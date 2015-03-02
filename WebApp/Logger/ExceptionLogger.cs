using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp.Logger
{
    public class ExceptionLogger : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            using (var db = new DataContainer())
            {
                var log = new ExceptionLog
                {
                    logDatetime = DateTime.Now,
                    logCurrentClass = context.GetType().FullName,
                    logException = context.Exception.Message,
                    logTrace = context.Exception.StackTrace
                };

                db.ExceptionLogs.Add(log);
                db.SaveChanges();
                db.Database.Connection.Close();
            }
        }
    }

}