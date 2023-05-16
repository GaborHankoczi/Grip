using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grip.Bll.Services.Interfaces
{
    /// <summary>
    /// Represents a generic REST interface for CRUD operations.
    /// </summary>
    /// <typeparam name="CreateDTO">The DTO type for creating a new resource.</typeparam>
    /// <typeparam name="UpdateDTO">The DTO type for updating an existing resource.</typeparam>
    /// <typeparam name="GetDTO">The DTO type for retrieving a resource.</typeparam>
    public interface IRestInterface<CreateDTO, UpdateDTO, GetDTO>
    {
        /// <summary>
        /// Retrieves all resources.
        /// </summary>
        /// <returns>A collection of GetDTO representing all resources.</returns>
        public Task<IEnumerable<GetDTO>> GetAll();

        /// <summary>
        /// Retrieves a resource by its ID.
        /// </summary>
        /// <param name="id">The ID of the resource to retrieve.</param>
        /// <returns>The GetDTO representing the retrieved resource.</returns>
        public Task<GetDTO> Get(int id);

        /// <summary>
        /// Creates a new resource.
        /// </summary>
        /// <param name="dto">The CreateDTO object containing the resource information.</param>
        /// <returns>The GetDTO representing the created resource.</returns>
        public Task<GetDTO> Create(CreateDTO dto);

        /// <summary>
        /// Updates an existing resource.
        /// </summary>
        /// <param name="dto">The UpdateDTO object containing the updated resource information.</param>
        public Task Update(UpdateDTO dto);

        /// <summary>
        /// Deletes a resource by its ID.
        /// </summary>
        /// <param name="id">The ID of the resource to delete.</param>
        public Task Delete(int id);
    }
}