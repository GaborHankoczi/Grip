using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Services.Interfaces
{
    public interface IRestInterface<CreateDTO, UpdateDTO, GetDTO>
    {
        public Task<IEnumerable<GetDTO>> GetAll();
        public Task<GetDTO> Get(int id);
        public Task<GetDTO> Create(CreateDTO dto);
        public Task Update(UpdateDTO dto);
        public Task Delete(int id);
    }
}