using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductVersion.Application.Features.Commands.ProductVersion;
using ProductVersion.Application.Features.Queries.ProductVersion;
using Product.Application.Wrappers.Requests;
using Product.Application.Wrappers.Responses;
using Product.WebApi.Controllers;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductVersion.WebApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductVersionController : BaseController
    {
        #region | CTOR |
        public ProductVersionController
            (IMediator mediator) : base(mediator)
        {
        }
        #endregion 

        #region | GetAllProductVersion |
        /// <summary>
        /// Get all ProductVersion
        /// </summary> 
        /// <returns></returns>
        [SwaggerOperation(Summary = "Get all ProductVersion")]
        [HttpGet]
        public async Task<IActionResult> GetAllProductVersion([FromQuery] GetAllProductVersionRequest m)
        {
            #region | Query |
            var query = new GetAllProductVersionQuery()
            {
                RequestModel = m
            };
            #endregion

            #region | Result |
            var result = await mediator.Send(query);
            if (result.Succeeded)
                return Ok(result);

            return NotFound(Result.Problem(result.Info));
            #endregion
        }
        #endregion

        #region | GetProductVersionById |
        /// <summary>
        /// Get ProductVersion by Id
        /// </summary>
        /// <param name="id">Member unique guid id</param>
        /// <returns></returns>
        [SwaggerOperation(Summary = "Get ProductVersion by Id")]
        [HttpPost]
        public async Task<IActionResult> GetProductVersionById([FromQuery] GetProductVersionByIdRequest m)
        {
            #region | Query |

            var query = new GetProductVersionByIdQuery()
            {
                RequestModel = m
            };
            #endregion

            #region | Result |
            var result = await mediator.Send(query);
            if (result.Succeeded)
                return Ok(result);

            return NotFound(Result.Problem(result.Info));
            #endregion
        }
        #endregion

        #region | CreateProductVersion |
        /// <summary>
        /// Create ProductVersion
        /// </summary>
        /// <param name="id">Member unique guid id</param>
        /// <returns></returns>
        [SwaggerOperation(Summary = "Create ProductVersion")]
        [HttpPost]
        public async Task<IActionResult> CreateProductVersion([FromQuery] CreateProductVersionRequest m)
        {
            #region | Query |

            var query = new CreateProductVersionCommand()
            {
                RequestModel = m
            };
            #endregion

            #region | Result |
            var result = await mediator.Send(query);
            if (result.Succeeded)
                return Ok(result);

            return NotFound(Result.Problem(result.Info));
            #endregion
        }
        #endregion

        #region | UpdateProductVersionById |
        /// <summary>
        /// Update ProductVersion by Id
        /// </summary>
        /// <param name="id">Member unique guid id</param>
        /// <returns></returns>
        [SwaggerOperation(Summary = "Update ProductVersion by Id")]
        [HttpPut]
        public async Task<IActionResult> UpdateProductVersionById([FromQuery] UpdateProductVersionByIdRequest m)
        {
            #region | Query |

            var query = new UpdateProductVersionCommand()
            {
                RequestModel = m
            };
            #endregion

            #region | Result |
            var result = await mediator.Send(query);
            if (result.Succeeded)
                return Ok(result);

            return NotFound(Result.Problem(result.Info));
            #endregion
        }
        #endregion

        #region | DeleteProductVersionById |
        /// <summary>
        /// Delete ProductVersion by Id
        /// </summary>
        /// <param name="id">Member unique guid id</param>
        /// <returns></returns>
        [SwaggerOperation(Summary = "Delete ProductVersion by Id")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProductVersionById([FromQuery] DeleteProductVersionByIdRequest m)
        {
            #region | Query |

            var query = new DeleteProductVersionCommand()
            {
                RequestModel = m
            };
            #endregion

            #region | Result |
            var result = await mediator.Send(query);
            if (result.Succeeded)
                return Ok(result);

            return NotFound(Result.Problem(result.Info));
            #endregion
        }
        #endregion
    }
}
