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
    public static class ParseWLServerLog
    {
        [FunctionName("ParseWLServerLog")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Received request via http trigger");
          
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            const string pattern = @"^####\<(?<Date>.*?)\>\s+\<(?<Severity>.*?)\>\s+\<(?<Subsystem>.*?)\>\s+\<(?<Machine>.*?)\>\s+\<(?<Server>.*?)\>\s+\<(?<ThreadID>.*?)\>\s+\<(?<UserID>.*?)\>\s+\<(?<TransactionID>.*?)\>\s+\<(?<DiagnosticID>.*?)\>\s+\<(?<RawTime>.*?)\>\s+\<(?<MessageID>.*?)\>\s+\<(?<MessageText>(.|\n)*?)\>";

            var res = new object();
            List<string> parseOptions = new List<string>();

            // custom field to show RawTime in as a UTC Timestamp - "yyyy-MM-dd HH:mm:ss.fff"
            if (req.Query["TimestampUTC"] == "true")
            {
                log.LogInformation("Found Query 'TimestampUTC=true'.  Adding 'WLSERVER_TIMESTAMPUTC' to ParseOptions");
                parseOptions.Add("WLSERVER_TIMESTAMPUTC");                
            }
            
            res = ArdyParseLog.ParseLog(pattern, requestBody, parseOptions);
                                   
            // Logging
            log.LogInformation("Completed Processing http request");
            
            // Finished so return results
            return res != null
                ? (ActionResult)new OkObjectResult(res)
                : new BadRequestObjectResult("Please pass a valid WebLogic Server Log file content in the request body");
        }
    }
}
