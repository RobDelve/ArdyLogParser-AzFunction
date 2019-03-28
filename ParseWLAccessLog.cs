using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Ardy.Tools.ArdyLogParser
{
    public static class ParseWLAccessLog
    {
        [FunctionName("ParseWLAccessLog")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Received request via http trigger");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            const string pattern = @"^(?<date>\d{4}-\d{2}-\d{2})\t(?<time>\d{2}:\d{2}:\d{2})\t(?<csmethod>\w*)\t(?<csuri>.*?)\t(?<scstatus>\d*)\t(?<timetaken>.*)\t(?<bytes>\d*)\t(?<cip>.*)\t(?<sc>.*?)\t(?<cs>.*?)\t(?<csuristem>.*)";

            List<string> parseOptions = new List<string>();
            var res = ArdyParseLog.ParseLog(pattern, requestBody, parseOptions);
           
            // Logging           
            log.LogInformation("Completed Processing http request");

            // Finished so return results
            return res != null
                ? (ActionResult)new OkObjectResult(res)
                : new BadRequestObjectResult("Please pass valid WebLogic Access Log file content in the request body");
        } 
    }
}
