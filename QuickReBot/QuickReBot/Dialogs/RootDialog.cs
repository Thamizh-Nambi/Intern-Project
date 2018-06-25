using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Configuration;
using QuickRe.DataClients.NetFramework.Connectors;
using QuickRe.Models;

namespace QuickReBot.Dialogs
{

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public static string FormattedToFind, FormattedRetreivedValue, FormattedThresholdValue, FormattedLob;
        public ResponseObject responseObject;
        public static List <string> AttributesToMail;
        public static List <string> Greetings;
        public Task StartAsync(IDialogContext context)
        {
            responseObject = new ResponseObject();
            AttributesToMail = new List<string>();
            AttributesToMail.Add("freshness");
            AttributesToMail.Add("responsetime");
            AttributesToMail.Add("replytime");
            AttributesToMail.Add("lagtime");

            Greetings = new List<string>();
            Greetings.Add("hey");
            Greetings.Add("hi");
            Greetings.Add("hello");
            Greetings.Add("yo");

            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {

            var activity = await result as Activity;
            responseObject.UserQuery = activity.Text;

            var RedirectTo = activity.Text.Split(' ').Skip(0).FirstOrDefault();
            var SecondWord = activity.Text.Split(' ').Skip(0).FirstOrDefault();
            var response = "";
            RedirectTo = "Azure";

            
            foreach (string greet in Greetings)
                if (activity.Text.ToLower().Contains(greet))
                {
                    response = "Welcome! Ask me any question and I'll fetch it for you!\nExamples:\n* Get replytime of OEM systems.\n* Get responsetime of Health department.\n* Request Database Access\n* Use OEM system";
                    response += "\nAt any time type HELP to access the guidelines in case you are stuck somewhere :)";
                    RedirectTo = "Greet";
                    break;
                }

            if (activity.Text.ToLower().Contains("request") && activity.Text.ToLower().Contains("access"))
            {
                RedirectTo = "Request";
                context.Call<object>(new DatabaseRequestDialog(), MessageReceivedAsync2);
                return;
            }

            if (RedirectTo == Constants.QA)
            { 

                QnAResponseObject QnAResponse = await QnADataClient.GetQnAResponse(responseObject.UserQuery);
                if (QnAResponse.answers[0].score < 50)
                    RedirectTo = Constants.Azure;
                else
                    response = QnAResponse.answers[0].answer;
            }

            else if (RedirectTo == Constants.Azure)
            {
                responseObject.SystemObject = AzureDataClient.GetSystemSelected(responseObject.UserQuery);
                if (responseObject.SystemObject == null)
                    await context.PostAsync("nulll af");

                if (SecondWord.ToLower().Equals("use"))
                {                    
                    context.Call<object>(new AzureSystemSelectedDialog(responseObject), MessageReceivedAsync2);
                    return;
                }
                else
                {                    
                    AzureDataClient.GetAzureResponse(responseObject,"RootDialog");
                    if (responseObject.RetreivedValue.Equals(""))
                    {                        
                        context.Call(new AzureSystemSelectedDialog(responseObject), MessageReceivedAsync2);
                        return;
                    }
                    else
                    {
                        int ValuePosition;
                        FormattedLob = responseObject.SystemObject.Lob.Substring(0, responseObject.SystemObject.Lob.Length - 2);
                        FormattedToFind = responseObject.ToFind;
                        ValuePosition = responseObject.RetreivedValue.IndexOf(':');
                        FormattedRetreivedValue = responseObject.RetreivedValue.Substring(0, ValuePosition);

                        await context.PostAsync("The "+FormattedToFind+" of "+FormattedLob +" is "+FormattedRetreivedValue);
                        foreach (string attributeToMail in AttributesToMail)
                        if(activity.Text.Contains(attributeToMail))
                        {
                            context.Call<object>(new SendMailDialog(responseObject), MessageReceivedAsync2);
                            return;
                        }
                        
                    }

                }                
            }
            
            if (RedirectTo != Constants.QA && RedirectTo != Constants.Azure && RedirectTo!="Greet")
                response = "Not able to retreive the information. Rephrase your question.\nAt any time type HELP to access the guidelines in case you are stuck somewhere :)";

            if(RedirectTo=="Greet" || RedirectTo==Constants.QA)
             await context.PostAsync(response);
             context.Wait(MessageReceivedAsync);
            
            
        }

        


        private async Task MessageReceivedAsync2(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync("Welcome Back, how may I assist you?\nAt any time type HELP to access the guidelines in case you are stuck somewhere :)");
            context.Done<object>(new object());

        }
    }
}