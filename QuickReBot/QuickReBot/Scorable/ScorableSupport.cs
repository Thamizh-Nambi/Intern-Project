using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace QuickReBot.Scorable
{
    public class ScorableSupport : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogStack DialogStackObject;

        public ScorableSupport(IDialogStack stack)
        {
            SetField.NotNull(out DialogStackObject, nameof(stack), stack);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }               

        protected override double GetScore(IActivity item, string state)
        {
            return state == "HELP PREPAREASYNC" ? 1 : 0;

        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state == "HELP PREPAREASYNC";

        }

        protected override Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var activity = item as IMessageActivity;
            var dialog = new ScorableSupportDialog();
            var interruption = dialog.Void(DialogStackObject);
            DialogStackObject.Call(interruption, null);
            return Task.CompletedTask;
        }
        

        protected override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var activity = item.AsMessageActivity();

            if (activity == null) return null;

            var messageText = activity.Text;

            if (messageText == "HELP")
                return "HELP PREPAREASYNC";

            return null;
        }
        
    }
}