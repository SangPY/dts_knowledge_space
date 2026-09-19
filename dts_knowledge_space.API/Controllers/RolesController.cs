using dts_knowledge_space.ViewModels;
using dts_knowledge_space.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dts_knowledge_space.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        //URL: POST: http://localhost:5001/api/roles
        [HttpPost]
        public async Task<IActionResult> PostRole(RoleVm roleVm)
        {
            var role = new IdentityRole()
            {
                Id = roleVm.Id,
                Name = roleVm.Name,
                NormalizedName = roleVm.Name.ToUpper()
            };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return CreatedAtAction(nameof(GetById), new { id = role.Id }, roleVm);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        //URL: GET: http://localhost:5001/api/roles/
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            var rolevms = roles.Select(r => new RoleVm()
            {
                Id = r.Id,
                Name = r.Name
            });
            return Ok(rolevms);
        }

        //URL: GET: http://localhost:5001/api/roles/paging?filter={filter}&pageIndex=1&pageSize=10
        // Single endpoint that supports optional filtering and optional pagination.
        // Use a distinct route to avoid conflicts with the parameterless GET action (GetRoles).
        [HttpGet("paging")]
        public async Task<IActionResult> GetRolesPaging(string? filter = null, int? pageIndex = null, int? pageSize = null)
        {
            var query = _roleManager.Roles.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter) || x.Name.Contains(filter));
            }

            // If pagination params provided, return paginated result
            if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
            {
                int totalRecords;
                try
                {
                    totalRecords = await query.CountAsync();
                }
                catch (InvalidOperationException)
                {
                    // In unit tests the provider may not support IAsyncQueryProvider; fall back to synchronous execution.
                    totalRecords = query.Count();
                }

                List<RoleVm> items;
                try
                {
                    items = await query
                        .Skip((pageIndex.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .Select(r => new RoleVm()
                        {
                            Id = r.Id,
                            Name = r.Name
                        })
                        .ToListAsync();
                }
                catch (InvalidOperationException)
                {
                    items = query
                        .Skip((pageIndex.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .Select(r => new RoleVm()
                        {
                            Id = r.Id,
                            Name = r.Name
                        })
                        .ToList();
                }

                var pagination = new Pagination<RoleVm>
                {
                    Items = items,
                    TotalRecords = totalRecords,
                };
                return Ok(pagination);
            }

            // Otherwise return full list
            List<RoleVm> roles;
            try
            {
                roles = await query
                    .Select(r => new RoleVm()
                    {
                        Id = r.Id,
                        Name = r.Name
                    })
                    .ToListAsync();
            }
            catch (InvalidOperationException)
            {
                roles = query
                    .Select(r => new RoleVm()
                    {
                        Id = r.Id,
                        Name = r.Name
                    })
                    .ToList();
            }

            return Ok(roles);
        }

        //URL: GET: http://localhost:5001/api/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var roleVm = new RoleVm()
            {
                Id = role.Id,
                Name = role.Name,
            };
            return Ok(roleVm);
        }

        //URL: PUT: http://localhost:5001/api/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, [FromBody] RoleVm roleVm)
        {
            if (id != roleVm.Id)
                return BadRequest();

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            role.Name = roleVm.Name;
            role.NormalizedName = roleVm.Name.ToUpper();

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                return NoContent();
            }
            return BadRequest(result.Errors);
        }

        //URL: DELETE: http://localhost:5001/api/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();
            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                //return NoContent();
                var rolevm = new RoleVm()
                {
                    Id = role.Id,
                    Name = role.Name
                };
                return Ok(rolevm);
            }
            return BadRequest(result.Errors);
        }
    }
}
