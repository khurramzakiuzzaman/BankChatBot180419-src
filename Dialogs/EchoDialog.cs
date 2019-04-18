using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Cafex.LiveAssist.Bot;
using System.Timers;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.ConnectorEx;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;
        static string TRANSFER_MESSAGE = "transfer to ";
        public string customerName;
        #region Old Code
        //private static Sdk sdk;
        //private static ChatContext chatContext;
        //private static string conversationRef;
        //private static Timer timer;


        //public async Task StartAsync(IDialogContext context)
        //{
        //    sdk = sdk ?? new Sdk(new SdkConfiguration()
        //    {
        //        AccountNumber = "25485974"
        //    });
        //    context.Wait(MessageReceivedAsync);
        //}

        //public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    var activity = await argument as Activity;

        //    if (chatContext != null)
        //    {
        //        // As chatContext is not null we already have an escalated chat.
        //        // Post the incoming message line to the escalated chat
        //        await sdk.PostLine(activity.Text, chatContext);
        //    }
        //    else if (activity.Text == "reset")
        //    {
        //        PromptDialog.Confirm(
        //            context,
        //            AfterResetAsync,
        //            "Are you sure you want to reset the count?",
        //            "Didn't get that!",
        //            promptStyle: PromptStyle.Auto);
        //    }
        //    else if (activity.Text.Contains("help"))
        //    {
        //        // "help" within the message is our escalation trigger.
        //        await context.PostAsync("Escalating to agent");
        //        await Escalate(activity); // Implemented in next step.
        //    }
        //    else
        //    {
        //        await context.PostAsync($"{this.count++}: You said {activity.Text}");
        //        context.Wait(MessageReceivedAsync);
        //    }
        //}
        //private async Task Escalate(Activity activity)
        //{
        //    // This is our reference to the upstream conversation
        //    conversationRef = JsonConvert.SerializeObject(activity.ToConversationReference());

        //    var chatSpec = new ChatSpec()
        //    {
        //        // Set Agent skill to target
        //        Skill = "BotEscalation",
        //        VisitorName = activity.From.Name
        //    };

        //    // Start timer to poll for Live Assist chat events
        //    if (timer == null)
        //    {
        //        timer = timer ?? new Timer(5000);
        //        // OnTimedEvent is implemented in the next step
        //        timer.Elapsed += (sender, e) => OnTimedEvent(sender, e);
        //        timer.Start();
        //    }

        //    // Request a chat via the Sdk    
        //    chatContext = await sdk.RequestChat(chatSpec);
        //}

        ////public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        ////{
        ////    var confirm = await argument;
        ////    if (confirm)
        ////    {
        ////        this.count = 1;
        ////        await context.PostAsync("Reset count.");
        ////    }
        ////    else
        ////    {
        ////        await context.PostAsync("Did not reset count.");
        ////    }
        ////    context.Wait(MessageReceivedAsync);
        ////}

        //async void OnTimedEvent(Object source, ElapsedEventArgs eea)
        //{
        //    if (chatContext != null)
        //    {
        //        // Create an upstream reply
        //        var reply = JsonConvert.DeserializeObject<ConversationReference>(conversationRef)
        //            .GetPostToBotMessage().CreateReply();

        //        // Create upstream connection on which to send reply 
        //        var client = new ConnectorClient(new Uri(reply.ServiceUrl));

        //        // Poll Live Assist for events
        //        var chatInfo = await sdk.Poll(chatContext);

        //        if (chatInfo != null)
        //        {
        //            // ChatInfo.ChatEvents will contain events since last call to poll.
        //            if (chatInfo.ChatEvents != null && chatInfo.ChatEvents.Count > 0)
        //            {
        //                foreach (ChatEvent e in chatInfo.ChatEvents)
        //                {
        //                    switch (e.Type)
        //                    {
        //                        // type is either "state" or "line".
        //                        case "line":
        //                            // Source is either: "system", "agent" or "visitor"
        //                            if (e.Source.Equals("system"))
        //                            {
        //                                reply.From.Name = "system";
        //                            }
        //                            else if (e.Source.Equals("agent"))
        //                            {
        //                                reply.From.Name = chatInfo.AgentName;

        //                            }
        //                            else
        //                            {
        //                                break;
        //                            }

        //                            reply.Type = "message";
        //                            reply.Text = e.Text;
        //                            client.Conversations.ReplyToActivity(reply);
        //                            break;

        //                        case "state":
        //                            // State changes
        //                            // Valid values: "waiting", "chatting", "ended"
        //                            if (chatInfo.State.Equals("ended"))
        //                            {
        //                                chatContext = null;
        //                            }
        //                            break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        //{
        //    var confirm = await argument;
        //    if (confirm)
        //    {
        //        this.count = 1;
        //        await context.PostAsync("Reset count.");
        //    }
        //    else
        //    {
        //        await context.PostAsync("Did not reset count.");
        //    }
        //    context.Wait(MessageReceivedAsync);
        //}
        #endregion

        // Live Assist custom channel data.
        public class LiveAssistChannelData
        {
            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("skill", NullValueHandling = NullValueHandling.Ignore)]
            public string Skill { get; set; }
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.ChannelId == "directline")
            {
                var laChannelData = message.GetChannelData<LiveAssistChannelData>();

                switch (laChannelData.Type)
                {
                    case "visitorContextData":
                        //process context data if required. This is the first message received so say hello.
                        //await context.PostAsync("Hi, I am an echo bot and will repeat everything you said.");
                        //string Welcomemessage = "Glad to talk to you. Welcome to iBot - your Virtual iBot.";
                        //await context.PostAsync(Welcomemessage);

                        //PromptDialog.Text(
                        //context: context,
                        //resume: ResumeLanguageOptions,
                        //prompt: $@"Which language you want to prefer?{Environment.NewLine} 1. English {Environment.NewLine} 2. Arabic",
                        //retry: "Sorry, I don't understand that.");

                        var reply2 = context.MakeMessage();
                        var transferTo2 = message.Text.Substring(TRANSFER_MESSAGE.Length);

                        reply2.ChannelData = new LiveAssistChannelData()
                        {
                            Type = "transfer",
                            Skill = "BotEscalation"
                        };

                        await context.PostAsync(reply2);

                        break;

                    case "systemMessage":
                        //react to system messages if required
                        break;

                    case "transferFailed":
                        //react to transfer failures if required
                        break;

                    case "otherAgentMessage":
                        //react to messages from a supervisor if required
                        break;

                    case "visitorMessage":
                        // Check for transfer message

                        if (message.Text.StartsWith(TRANSFER_MESSAGE))
                        {
                            var reply = context.MakeMessage();
                            var transferTo = message.Text.Substring(TRANSFER_MESSAGE.Length);

                            reply.ChannelData = new LiveAssistChannelData()
                            {
                                Type = "transfer",
                                Skill = "BotEscalation"
                            };

                            await context.PostAsync(reply);
                        }
                        else if (message.Text.StartsWith("hi") || message.Text.StartsWith("hello") || message.Text.StartsWith("Hi"))
                        {
                            if (customerName == null)
                            {
                                string Welcomemessage2 = "Glad to talk to you. Welcome to iBot - your Virtual Wasl Property Consultant.";
                                await context.PostAsync(Welcomemessage2);

                                PromptDialog.Text(
                                context: context,
                                resume: ResumeLanguageOptions,
                                prompt: $@"Which language you want to prefer?{Environment.NewLine} 1. English {Environment.NewLine} 2. Arabic",
                                retry: "Sorry, I don't understand that.");
                            }
                            else
                            {
                                string message23 = "Tell me " + customerName + ". How i can help you?";
                                await context.PostAsync(message23);
                                context.Wait(MessageReceivedAsync);
                            }
                        }
                        else if (message.Text.Contains("issue") || message.Text.Contains("problem"))
                        {
                            //PromptDialog.Text(
                            //   context: context,
                            //   resume: CustomerRepeatChecking,
                            //   prompt: "May i know your mobile number for verification purpose?",
                            //   retry: "Sorry, I don't understand that.");
                        }
                        else if (message.Text.Contains("sell") || message.Text.Contains("buy") || message.Text.Contains("property"))
                        {
                            PromptDialog.Text(
                               context: context,
                               resume: ResumeLanguageOptions,
                               prompt: $@"Which language you want to prefer?{Environment.NewLine} 1. English {Environment.NewLine} 2. Arabic",
                               retry: "Sorry, I don't understand that.");
                        }
                        else
                        {
                            await context.PostAsync("You said: " + message.Text);
                           
                        }
                        break;

                    default:
                        await context.PostAsync("This is not a Live Assist message " + laChannelData.Type);
                        break;
                }
            }

            else if (message.Text == "reset")
            {
                //PromptDialog.Confirm(
                //    context,
                //    AfterResetAsync,
                //    "Are you sure you want to reset the count?",
                //    "Didn't get that!",
                //    promptStyle: PromptStyle.Auto);
            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }
        public async Task ResumeLanguageOptions(IDialogContext context, IAwaitable<string> argument)
        {
            
        }

    }
}