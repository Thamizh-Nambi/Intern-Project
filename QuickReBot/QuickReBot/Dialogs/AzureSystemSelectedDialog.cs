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
    public class AzureSystemSelectedDialog : IDialog<object>
    {
        public static string FormattedToFind, FormattedRetreivedValue, FormattedThresholdValue, FormattedLob;

        public ResponseObject responseObject;
        public AzureSystemSelectedDialog(ResponseObject resObj)
        {
            responseObject = resObj;
            FormattedLob = responseObject.SystemObject.Lob.Substring(0, responseObject.SystemObject.Lob.Length - 2);
        }
        public async Task StartAsync(IDialogContext context)
        {

            await context.PostAsync("You selected the system : "+ FormattedLob + "\nWhat do you want to find in "+ FormattedLob + "?\n" );
            context.Wait(MessageReceivedAsync);


            // State transition - complete this Dialog and remove it from the stack
            
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            await context.PostAsync("You chose : " + activity.Text+".\nJust a minute. ");
            responseObject.ToFind = activity.Text;
            responseObject.RetreivedValue = "";

            AzureDataClient.GetAzureResponse(responseObject,"azuredialog");

            if(responseObject.RetreivedValue=="")
            {
                await context.PostAsync("The " + FormattedToFind + " of " + FormattedLob + " is not found. Please retry.");    
            }
            else
            {
                int ValuePosition;
                FormattedToFind = activity.Text;
                ValuePosition = responseObject.RetreivedValue.IndexOf(':');
                FormattedRetreivedValue = responseObject.RetreivedValue.Substring(0, ValuePosition);

                await context.PostAsync("The " + FormattedToFind + " of " + FormattedLob + " is " + FormattedRetreivedValue); ;


                foreach (string attributeToMail in RootDialog.AttributesToMail)
                    if (activity.Text.Contains(attributeToMail))
                    {
                        context.Call(new SendMailDialog(responseObject), AfterMailDeciderAsync);
                        return;
                    }
            }
            
            
            await AfterMailDeciderAsync(context,result);
            
        }

        private async Task AfterMailDeciderAsync(IDialogContext context, IAwaitable<object> result)
        {
            PromptDialog.Choice(
                  context: context,
                  resume: ContinueSameSystemDeciderAsync,
                  options: new List<string>() { "Yes, continue with"+ FormattedLob, "No, change the system" },
                  prompt: "Do you want to continue with the same system or change it?",
                  retry: "Selected feature not available . Please try again.",
                  promptStyle: PromptStyle.Auto
              );
        }

        private async Task ContinueSameSystemDeciderAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;
            if (activity.ToString()== "Yes, continue with" + FormattedLob)
                await StartAsync(context);
            else
                context.Done<object>(new object());
        }
    }
}