using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LRC_NET_Framework.Models
{
    /*
 @Author:Alex
 @Date:26/05/2018
 @description:constant Error codes with error messages
 */
    public class Error
    {
        public int errCode { get; set; }

        //String describing the error
        public string errMsg { get; set; }

        //Number of seconds we should stop sending requests for use with Error Code 4
        [JsonProperty("timeout", NullValueHandling = NullValueHandling.Ignore)]
        public string timeout { get; set; }
    }
    /*
   @Author:Raj Tiwari
   @Date:22/05/2018
   @description:Constant error messages
   */
    public class ErrorDetail
    {

        //--------------------------------------------------------------------------
        // Constant error codes
        public const int UnknownError = 101;
        public const int DataImportError = 102;
        public const int InvalidCredential = 103;
        public const int InvalidEmail = 104;
        public const int InvalidMobile = 105;
        public const int Timeoutrequested = 106;
        public const int RecoverableError = 107;
        public const int Success = 108;
        public const int Failed = 109;
        public const int SmtpSuccess = 110;
        public const int SmtpFailed = 111;
        public const int EmailValidationRequired = 112;


        private static Hashtable ErrorMessages = new Hashtable();

        //--------------------------------------------------------------------------
        /// <summary>
        /// Constructor - add error code and message in hash table
        /// </summary>
        static ErrorDetail()
        {
            ErrorMessages.Add(UnknownError, "We are sorry. We have technical issue");
            ErrorMessages.Add(DataImportError, "Wrong imported data");
            ErrorMessages.Add(InvalidCredential, "Invalid email or mobile");
            ErrorMessages.Add(InvalidEmail, "Please enter valid email");
            ErrorMessages.Add(InvalidMobile, "Please enter valid mobile");
            ErrorMessages.Add(Timeoutrequested, "Timeout requested");
            ErrorMessages.Add(Success, "Success");
            ErrorMessages.Add(Failed, "Wrong Request");
            ErrorMessages.Add(SmtpSuccess, "Message sent successfully");
            ErrorMessages.Add(SmtpFailed, "Failed to deliver message");
            ErrorMessages.Add(EmailValidationRequired, "Email not verified");

        }

        /// <summary>
        /// Get meessage associated with error code
        /// </summary>
        /// <param name="hResult">int error code</param>
        /// <returns>string error message</returns>
        public static String GetMsg(int hResult)
        {
            String eventMessage = ErrorMessages[hResult] as String;

            if (eventMessage == null)
            {
                eventMessage = "Server was unable to process request.";
            }

            return eventMessage;
        }
    }
    /*
@Author:Raj Tiwari
@Date:22/05/2018
@description:Final response properties
*/
    public class ResponseStatusData
    {
        public int StatusCode { get; set; }
        public dynamic[] data { get; set; }
        public Error error { get; set; }
    }
    public class DbErrorMessage
    {
        public int errCode { get; set; }
        public string errMsg { get; set; }
    }

 /*   @Author:Alex
@Date:26/05/2018
@description:This method used for create final response with serialized json data for all request
 */
    public static class ApiJsonResponse
    {
        public static string GetJsonWithStatusCode(int statusCode,Error Error,dynamic data,int Flag=0)
        {
            ResponseStatusData res = new ResponseStatusData();
            try
            {
                if (Flag == 0)
                {
                    res.StatusCode = statusCode;
                    res.error = Error;
                    res.data = data == null ? new dynamic[] { } : new dynamic[] { data };
                    return JsonConvert.SerializeObject(res);
                }
                else
                {
                    return "{\"StatusCode\":" + statusCode + ",\"data\":" + data + ",\"error\":\"\"}";
                }
            }
            catch(Exception ex)
            {
                Error err = new Error();
                err.errCode = ErrorDetail.Failed;
                err.errMsg = ex.Message;
                res.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                res.error = err;
                res.data = new dynamic[] { };
                return JsonConvert.SerializeObject(res);
            }           
        }
    }
}
