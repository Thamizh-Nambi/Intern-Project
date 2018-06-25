using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QuickReBot.Scorable
{
    [Serializable]
    public class ScorableSupportDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.PostAsync($"\n\nHELP GUIDE\n* Ask Query of the form: (System) (Property)\n* Type 'Use' (system name) to enter into conversational mode.\n* Type 'Request DB access' to gain access to a database");
            context.Done<object>(new object());
            return Task.CompletedTask;
        }
    }
}