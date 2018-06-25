using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using QuickRe.DataClients.NetFramework.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QuickReBot.Dialogs
{
    [Serializable]
    public class DatabaseRequestDialog : IDialog<object>
    {
        public RequestDbObject requestDbObject;
        

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("*Database Request");
            requestDbObject = new RequestDbObject();
            requestDbObject.AccessType = "Read Only";                     

            await GetType(context);
            //context.Done<object>(new object());
        }

        

        public async Task GetType(IDialogContext context)
        {
            PromptDialog.Choice(
              context: context,
              resume: GetEnvUrl,
              options: new List<string>() { "Environment", "Production Report","BACK" },
              prompt: "Choose the type",
              retry: "Selected environment not available . Please try again.",
              promptStyle: PromptStyle.Auto
              );

            //await context.PostAsync("*\nEnter the type : \n1) Environment\n2) Production Report");
            //context.Wait(GetEnvUrl);              
        }

        public async Task GetEnvUrl(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;

            if (activity.ToString()=="BACK") 
                await GetType(context);
            else
            {
                
                if (activity.ToString() == "Environment")
                {
                    await context.PostAsync("*Enter the name of the Environment ");
                    requestDbObject.TypeChoice = "1";
                }
                    
                else if (activity.ToString() == "Production Report")
                {
                    await context.PostAsync("*Enter the URL of the report ");
                    requestDbObject.TypeChoice = "2";
                }
                   
                context.Wait(GetQuantity);
            }
        }

        public async Task GetQuantity(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Text.Equals("back", StringComparison.OrdinalIgnoreCase))
                await GetEnvUrl(context, result);
            else
            {
                if (requestDbObject.TypeChoice == "1")
                    requestDbObject.RequestEnvironment = activity.Text;
                else if (requestDbObject.TypeChoice == "2")
                    requestDbObject.RequestUrl = activity.Text;

                PromptDialog.Choice(
                  context: context,
                  resume: GetAlias,
                  options: new List<string>() { "Security Group", "Individual Access" },
                  prompt: "Choose the type",
                  retry: "Selected environment not available . Please try again.",
                  promptStyle: PromptStyle.Auto
                  );
                
            }
        }

        public async Task GetAlias(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;

            if (activity.ToString()=="BACK")
                await GetQuantity(context, result);
            else
            {
                if (activity.ToString() == "Security Group")
                {
                    await context.PostAsync("*Enter the security group Alias id");
                    requestDbObject.QuantityChoice = "1";
                }                    
                else if (activity.ToString() == "Individual Access")
                {
                    await context.PostAsync("*Enter the Individual Alias id");
                    requestDbObject.QuantityChoice = "2";
                }                            
                context.Wait(GetBusinessJustification);
            }
        }

        public async Task GetBusinessJustification(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;

            if (activity.Text.Equals("back", StringComparison.OrdinalIgnoreCase))
                await GetQuantity(context, result);
            else
            {
                requestDbObject.Alias = activity.Text;

                await context.PostAsync("*Enter the reason for getting the permission.");
                context.Wait(Finish);
            }
        }
        public async Task Finish(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (activity.Text.Equals("back", StringComparison.OrdinalIgnoreCase))
                await GetBusinessJustification(context, result);
            else
            {
                requestDbObject.BusinessJustification = activity.Text;

                await context.PostAsync("MAIL SENT");
                context.Done<object>(new object());
            }
        }
    }
}