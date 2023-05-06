using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grip.Bll.DTO;
using Grip.Bll.Exceptions;
using Grip.Bll.Services.Interfaces;
using Grip.DAL;
using Grip.DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Grip.Bll.Services
{
    public class PassiveTagService : IPassiveTagService
    {

        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PassiveTagService(IMapper mapper, ApplicationDbContext context, UserManager<User> userManager)
        {
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
        }
        public async Task<PassiveTagDTO> Create(CreatePassiveTagDTO dto)
        {
            var passiveTagToCreate = _mapper.Map<PassiveTag>(dto);
            if (_context.PassiveTags.Any(p => p.SerialNumber == passiveTagToCreate.SerialNumber))
            {
                throw new BadRequestException();
            }
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
            {
                throw new NotFoundException();
            }
            passiveTagToCreate.User = user;
            _context.PassiveTags.Add(passiveTagToCreate);
            await _context.SaveChangesAsync();
            return await Get(passiveTagToCreate.Id);
        }

        public async Task Delete(int id)
        {
            var passiveTag = await _context.PassiveTags.FindAsync(id);
            if (passiveTag == null)
            {
                throw new NotFoundException();
            }

            _context.PassiveTags.Remove(passiveTag);
            await _context.SaveChangesAsync();
        }

        public async Task<PassiveTagDTO> Get(int id)
        {
            var passiveTag = await _context.PassiveTags.Include(p => p.User).Where(p => p.Id == id).FirstOrDefaultAsync();

            if (passiveTag == null)
            {
                throw new NotFoundException();
            }
            return _mapper.Map<PassiveTagDTO>(passiveTag) with { User = _mapper.Map<UserInfoDTO>(passiveTag.User) };
        }

        public async Task<IEnumerable<PassiveTagDTO>> GetAll()
        {
            return (await _context.PassiveTags.Include(p => p.User).ToListAsync())
            .Select(
                p => _mapper.Map<PassiveTagDTO>(p)
                with
                { User = _mapper.Map<UserInfoDTO>(p.User) }).ToList();
        }

        public async Task Update(UpdatePassiveTagDTO dto)
        {

            var passiveTagEntity = await _context.PassiveTags.Include(p => p.User).Where(p => p.Id == dto.Id).FirstOrDefaultAsync();
            if (_context.PassiveTags.Any(p => p.SerialNumber == dto.SerialNumber && p.Id != dto.Id))
            {
                throw new BadRequestException();
            }
            if (passiveTagEntity == null)
            {
                throw new NotFoundException();
            }

            _mapper.Map(dto, passiveTagEntity);
            if (passiveTagEntity.User.Id != dto.UserId)
            {
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    throw new NotFoundException();
                }
                passiveTagEntity.User = user;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PassiveTagExists(dto.Id))
                {
                    throw new NotFoundException();
                }
                else
                {
                    throw new DbConcurrencyException();
                }
            }
        }

        private bool PassiveTagExists(int id)
        {
            return (_context.PassiveTags?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}