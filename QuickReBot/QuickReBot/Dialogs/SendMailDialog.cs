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
    public class SendMailDialog : IDialog<object>
    {
        public ResponseObject responseObject;

        public static string helpdesk;
        public static string problem;
        public static string subject;
        public static string fromid;

        public static string FormattedToFind, FormattedRetreivedValue, FormattedThresholdValue, FormattedLob;

        public SendMailDialog(ResponseObject res)
        {
            responseObject = res;

            helpdesk = res.SystemObject.HelpDesk;
            helpdesk = helpdesk.Substring(0, helpdesk.Length - 7);
            problem = res.ToFind;
            subject = "Regarding " + res.ToFind;
            fromid = "bot@microsoft.com";
        }
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(AzureDataClient.ExtractAnswer(responseObject, "threshold"));
            string ThresholdValue = responseObject.ThresholdValue;

            int ValuePosition;
            ValuePosition = responseObject.RetreivedValue.IndexOf(' ');
            int CurrentValue = int.Parse(responseObject.RetreivedValue.Substring(0, ValuePosition));

            ValuePosition = responseObject.ThresholdValue.IndexOf(' ');
            int ThreshValue = int.Parse(responseObject.ThresholdValue.Substring(0, ValuePosition));


            //Formatting
            FormattedLob = responseObject.SystemObject.Lob.Substring(0, responseObject.SystemObject.Lob.Length - 2);
            FormattedToFind = responseObject.ToFind;
            ValuePosition = responseObject.RetreivedValue.IndexOf(':');
            FormattedRetreivedValue = responseObject.RetreivedValue.Substring(0, ValuePosition);
            ValuePosition = responseObject.ThresholdValue.IndexOf(':');
            FormattedThresholdValue = responseObject.ThresholdValue.Substring(0, ValuePosition);



            if (CurrentValue < ThreshValue)
            {
                await context.PostAsync("The value of " + FormattedToFind + " ( "+FormattedRetreivedValue+" ) is within the threshold value of " + FormattedThresholdValue);
                await context.PostAsync("MAIL INFO\n*Sender: " + fromid + "\n*Receiver: " + helpdesk + "\n*Subject: " + subject + "\n");
                PromptDialog.Choice(
                  context: context,
                  resume: MessageReceivedAsync,
                  options: new List<string>() { "Yes, Send a mail", "No, Dont send" },
                  prompt: "Do you want to send a mail to the helpdesk anyway?",
                  retry: "Selected feature not available . Please try again.",
                  promptStyle: PromptStyle.Auto
              );

            }
            else
            {
                await context.PostAsync("It looks like the value of " + FormattedToFind + " ( " + FormattedRetreivedValue + " )  is higher than the threshold value of " + FormattedThresholdValue);
                await context.PostAsync("MAIL INFO\n*Sender: " + fromid + "\n*Receiver: " + helpdesk + "\n*Subject: " + subject + "\n");

                PromptDialog.Choice(
                  context: context,
                  resume: MessageReceivedAsync,
                  options: new List<string>() { "Yes, Send a mail", "No, Dont send" },
                  prompt: "Do you want to send a mail to the helpdesk regarding the deviation?",
                  retry: "Selected feature not available . Please try again.",
                  promptStyle: PromptStyle.Auto
              );

            }
            

            // State transition - complete this Dialog and remove it from the stack
            
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result;
            if (activity.ToString() == "Yes, Send a mail")
            {
                var OutputMessage = context.MakeMessage();
                var receiptCard = new ReceiptCard
                {
                    Title = "Message Sent Sucessfully",
                    Facts = new List<Fact> { new Fact("Sender", fromid), new Fact("Receiver", helpdesk), new Fact("Subject", subject) },
                    Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "Open Account",
                        "https://www.outlook.com")
                }
                };

                var attachment = receiptCard.ToAttachment();
                OutputMessage.Attachments.Add(attachment);
                await context.PostAsync(OutputMessage);
            }
            else
            {
                await context.PostAsync("MAIL NOT SENT ");

            }
            context.Done<object>(new object());

        }

    }
}