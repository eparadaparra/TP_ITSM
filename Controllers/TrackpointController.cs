using Microsoft.AspNetCore.Mvc;
using TP_ITSM.Models;
using TP_ITSM.Models.Trackpoint;
using TP_ITSM.Services.Trackpoint;

namespace TP_ITSM.Controllers
{
    [Route("api/Trackpoint")]
    [ApiController]
    public class TrackpointController : ControllerBase
    {
        private readonly IServices _services;
        public TrackpointController(IServices services)
        {
            _services = services;
        }

        #region Token
        [HttpGet]
        [Route("Token")]
        public async Task<IActionResult> GetToken()
        {
            var response = await _services.GetToken();

            return Ok(response);
        }
        #endregion

        #region All Customers
        [HttpGet]
        [Route("Customer/All")]
        public async Task<IActionResult> GetAllCustomer()
        {
            var response = await _services.GetAllCustomer();

            return Ok(response);
        }
        #endregion

        #region Customers By ID
        [HttpPost]
        [Route("Customer/ID")]
        public async Task<IActionResult> GetCustomer([FromBody] FirebaseIdRequest body)
        {
            string id = body?.id?.Trim() ?? string.Empty;
            string etiqueta = id.Count() == 4 ? "client_id" : "id";
            object newObj = new Dictionary<string, string>
            {
                [etiqueta] = id
            };

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { status = StatusCodes.Status400BadRequest, message = "custID is required", data = "" });
            }

            var (success, result) = await _services.GetCustomer(newObj);

            if (success)
            {
                //var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                //return Ok(jsonResult);

                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

        #region Add Customers
        [HttpPost]
        [Route("Customer/Add")]
        public async Task<IActionResult> AddCustomer(TpCustomerResponse body )
        {
            if ( body is null)
            {
                return BadRequest(new { status = StatusCodes.Status400BadRequest, message = "Información requerida", data = "" });
            }

            var (success, result) = await _services.InsUpdDelCustomer(body, "INS");

            if (success)
            {
                //var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                //return Ok(jsonResult);
                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

        #region Update Customers
        [HttpPut]
        [Route("Customer/Update")]
        public async Task<IActionResult> UpdateCustomer(TpCustomerResponse body)
        {
            if (body is null)
            {
                return BadRequest(new { status = StatusCodes.Status400BadRequest, message = "Información requerida", data = "" });
            }

            var (success, result) = await _services.InsUpdDelCustomer(body, "UPD");

            if (success)
            {
                //var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                //return Ok(jsonResult);

                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

        #region Customers Delete
        [HttpDelete]
        [Route("Customer/Delete")]
        public async Task<IActionResult> DeleteCustomer([FromBody] FirebaseIdRequest body)
        {
            string id = body?.id?.Trim() ?? string.Empty;
            string etiqueta = id.Count() == 4 ? "client_id" : "id";
            object newObj = new Dictionary<string, string>
            {
                [etiqueta] = id
            };

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { status = StatusCodes.Status400BadRequest, message = "Customer ID is required", data = "" });
            }

            var (success, result) = await _services.InsUpdDelCustomer(newObj, "DEL");

            if (success)
            {
                //var jsonResult = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(result);
                //return Ok(jsonResult);

                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

        #region Get Event Activity
        [HttpPost]
        [Route("Activity/FirebaseId")]
        public async Task<IActionResult> GetActivity([FromBody] FirebaseId body)
        {
            string id = body?.firebaseId?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { status = StatusCodes.Status400BadRequest, message = "FirebaseId is required", data = "" });
            }

            var (success, result) = await _services.GetActivityTP(id);

            if (success)
            {
                return Content(result, "application/json");
            }
            else
            {
                ErrorResponse error = new ErrorResponse();
                error.Mensaje = result;
                return Ok(error);
            }
        }
        #endregion

    }
}
