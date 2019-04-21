using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Cafex.LiveAssist.Bot;
using System.Timers;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.ConnectorEx;
using System.Collections.Generic;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;
        #region Old Code
        private static Sdk sdk;
        private static ChatContext chatContext;
        private static string conversationRef;
        private static Timer timer;
        public string accountNumber;
        public string lostDate;
        public string lostCity;
        public string complaint;
        public string phone;
        public string email;
        public string customerName;
        public decimal loanAmount;
        public decimal loanAmountMonths;

        public async Task StartAsync(IDialogContext context)
        {
            sdk = sdk ?? new Sdk(new SdkConfiguration()
            {
                AccountNumber = "25485974"
            });
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = await argument as Activity;

            if (chatContext != null)
            {
                // As chatContext is not null we already have an escalated chat.
                // Post the incoming message line to the escalated chat
                await sdk.PostLine(activity.Text, chatContext);
            }
            else if (activity.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (activity.Text.Contains("help") || activity.Text.Contains("transfer"))
            {
                // "help" within the message is our escalation trigger.
                await context.PostAsync("Escalating to agent");
                await Escalate(activity); // Implemented in next step.
            }
            else
            {
                //await context.PostAsync($"{this.count++}: You said {activity.Text}");
                //context.Wait(MessageReceivedAsync);

                string message = "Welcome to iBank.";
                await context.PostAsync(message);

                //PromptDialog.Choice(context, ResumeBankServicesOptionsAsync,
                //    new List<string>()
                //    { "English",
                //  "Arabic"
                //    }, "Please select your langauge");

                var feedback = ((Activity)context.Activity).CreateReply("Please select your langauge?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "English", Type=ActionTypes.PostBack, Value=$"English" },
                    new CardAction(){ Title = "Arabic", Type=ActionTypes.PostBack, Value=$"Arabic" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ResumeBankServicesOptionsAsync);

            }
        }
        public virtual async Task ResumeBankServicesOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //PromptDialog.Choice(context, ResumeBankServicesAsync,
            //    new List<string>()
            //    {
            //        "Phone Banking",
            //        "Lost Cards",
            //        "Internet Banking",
            //        "Products",
            //        "Suggestions and Complaints"
            //    },
            //    "Thank you, please select your desired service.");

            var feedback = ((Activity)context.Activity).CreateReply("Thank you, please select your desired service.");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Phone Banking", Type=ActionTypes.PostBack, Value=$"PhoneBanking" },
                    new CardAction(){ Title = "Lost Cards", Type=ActionTypes.PostBack, Value=$"LostCards" },
                     new CardAction(){ Title = "Internet Banking", Type=ActionTypes.PostBack, Value=$"InternetBanking" },
                      new CardAction(){ Title = "Products", Type=ActionTypes.PostBack, Value=$"Products" },
                       new CardAction(){ Title = "Suggestions and Complaints", Type=ActionTypes.PostBack, Value=$"SuggestionsandComplaints" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(ResumeBankServicesAsync);
        }
        public virtual async Task ResumeBankServicesAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var result = await argument;
            //string selection = result;
            if (result.Text.Contains("PhoneBanking"))
            {

                PromptDialog.Text(
                context: context,
                resume: ResumePhoneBankingOptionsAsync,
                prompt: "Please enter your 4 digit phone banking number or account number",
                retry: "Sorry, I don't understand that.");
            }
            else if (result.Text.Contains("LostCards"))
            {
                //PromptDialog.Choice(context, ResumeBankValidationAsync,
                //    new List<string>()
                //    {
                //        "Debit Card",
                //        "Credit Card"
                //    },
                //    "Please select the one you lost?");
                var feedback = ((Activity)context.Activity).CreateReply("Please select the one you lost?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Debit Card", Type=ActionTypes.PostBack, Value=$"DebitCard" },
                    new CardAction(){ Title = "Credit Card", Type=ActionTypes.PostBack, Value=$"CreditCard" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ResumeBankValidationAsync);
            }
            else if (result.Text.Contains("Internet Banking"))
            {
                PromptDialog.Text(
               context: context,
               resume: CustomerNameFromGreeting,
               prompt: "Please enter your 4 digit phone banking number or account number",
               retry: "Sorry, I don't understand that.");
            }
            else if (result.Text.Contains("Products"))
            {
                //PromptDialog.Choice(context, ResumeBankProductOptionsAsync,
                //   new List<string>()
                //   {
                //        "Apply for a loan",
                //        "Open an account",
                //        "Know interest rates",
                //        "Forex"
                //   },
                //   "Please select the desired service?");

                var feedback = ((Activity)context.Activity).CreateReply("Please select the desired service?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Apply for a loan", Type=ActionTypes.PostBack, Value=$"Applyforaloan" },
                    new CardAction(){ Title = "Open an account", Type=ActionTypes.PostBack, Value=$"Openanaccount" },
                    new CardAction(){ Title = "Know interest rates", Type=ActionTypes.PostBack, Value=$"Knowinterestrates" },
                    new CardAction(){ Title = "Forex", Type=ActionTypes.PostBack, Value=$"Forex" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ResumeBankProductOptionsAsync);
            }
            else if (result.Text.Contains("SuggestionsandComplaints"))
            {
                PromptDialog.Text(
            context: context,
            resume: CustomerName,
            prompt: "What is your complaint/suggestion?",
            retry: "Sorry, I don't understand that.");
            }
        }
        public virtual async Task ResumeBankProductOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var selection = await argument;
            if (selection.Text.Contains("Applyforaloan"))
            {
                PromptDialog.Text(
            context: context,
            resume: CustomerLoanProcess,
            prompt: "Please enter your 4 digit phone banking number or account number?",
            retry: "Sorry, I don't understand that.");
            }
            else if (selection.Text.Contains("Openanaccount"))
            {

            }
            else if (selection.Text.Contains("Knowinterestrates"))
            {

            }
            else if (selection.Text.Contains("Forex"))
            {

            }

        }
        public async Task CustomerLoanProcess(IDialogContext context, IAwaitable<string> result)
        {
            string message = "Thanks Sekhar for validating your account number.";
            await context.PostAsync(message);

            PromptDialog.Text(
           context: context,
           resume: LoanMonthsExecution,
           prompt: "Please enter the amount you want to apply.",
           retry: "Sorry, I don't understand that.");
        }
        public async Task LoanMonthsExecution(IDialogContext context, IAwaitable<string> result)
        {
            string Amount = await result;
            loanAmount = Convert.ToDecimal(Amount);

            PromptDialog.Text(
            context: context,
            resume: LoanMobileNoExecution,
            prompt: "Please enter the Number of months to repay the amount.",
            retry: "Sorry, I don't understand that.");
        }
        public async Task LoanMobileNoExecution(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            loanAmountMonths = Convert.ToDecimal(response);

            PromptDialog.Text(
            context: context,
            resume: LoanExecution,
            prompt: "What is the best number to contact you?",
            retry: "Sorry, I don't understand that.");
        }
        public async Task LoanExecution(IDialogContext context, IAwaitable<string> result)
        {
            phone = await result;



            await context.PostAsync($@"Thank you for your interest, your request has been logged. Our Sales team will get back to you shortly.
                                    {Environment.NewLine}Your Loan request  summary:
                                    {Environment.NewLine}Loan Amount: {loanAmount},
                                    {Environment.NewLine}Number of Months: {loanAmountMonths},
                                    {Environment.NewLine}Phone Number: {phone}");

            // string refno = CRMService.CreateLead(loanAmount, loanAmountMonths, phone);

            var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AnythingElseHandler);
        }
        public virtual async Task ResumeBankValidationAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            PromptDialog.Text(
            context: context,
            resume: ResumeBankOptionsAsync,
            prompt: "Please enter your 4 digit phone banking number or account number?",
            retry: "Sorry, I don't understand that.");


        }
        public virtual async Task ResumeBankOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            //PromptDialog.Confirm(
            //context: context,
            //resume: LostorBlockExecution,
            //prompt: "Would you like me to suspend your card and issue new card?",
            //retry: "Sorry, I don't understand that.");

            var feedback = ((Activity)context.Activity).CreateReply("Would you like me to suspend your card and issue new card?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(LostorBlockExecution);
        }
        public async Task LostorBlockExecution(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var answer = await argument;
            if (answer.Text.Contains("Yes"))
            {
                string message = $"OK. I will need some information to process your request.";
                await context.PostAsync(message);

                PromptDialog.Text(
                context: context,
                resume: LostDate,
                prompt: "When did you lose your card?",
                retry: "Sorry, I don't understand that.");
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);
            }
        }
        public async Task LostDate(IDialogContext context, IAwaitable<string> argument)
        {
            var answer = await argument;
            lostDate = answer;

            PromptDialog.Text(
               context: context,
               resume: LostInformation,
               prompt: "Which city did you lose your card in?",
               retry: "Sorry, I don't understand that.");
        }
        public async Task LostInformation(IDialogContext context, IAwaitable<string> argument)
        {
            var answer = await argument;
            lostCity = answer;



            await context.PostAsync($@" {Environment.NewLine}The below is your card information:
                                     {Environment.NewLine}Card Number:2568-2321-628-2212
                                    {Environment.NewLine}Lost Date: {lostDate},
                                    {Environment.NewLine}Lost City: {lostCity}");
           // string refno = CRMService.CreateCaseRegistrationForCard(lostDate, lostCity);

            //PromptDialog.Confirm(
            //   context: context,
            //   resume: CardInformation,
            //   prompt: "Please confirm your request for suspending the lost card?",
            //   retry: "Sorry, I don't understand that.");

            var feedback = ((Activity)context.Activity).CreateReply("Please confirm your request for suspending the lost card?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(CardInformation);
        }
        public async Task CardInformation(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var answer = await argument;
            if (answer.Text.Contains("Yes"))
            {
                string message = $"Thanks. Your card is now suspended and the new card will be delivered to your registered address.";
                await context.PostAsync(message);

                var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(AnythingElseHandler);
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);
            }
        }
        public virtual async Task ResumePhoneBankingOptionsAsync(IDialogContext context, IAwaitable<string> argument)
        {
            accountNumber = await argument;

            string message = "Thanks Sekhar for validating your account number.";
            await context.PostAsync(message);

            //PromptDialog.Choice(context, ResumePhoneOptionsAsync, 
            //    new List<string>()
            //    {
            //        "Balance Enquiry",
            //        "Last 5 Transactions",
            //        "Statement & Check book Request",
            //        "Loan Account Information"
            //    }, 
            //    "Please select the desired service?");

            var feedback = ((Activity)context.Activity).CreateReply("Please select the desired service?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Balance Enquiry", Type=ActionTypes.PostBack, Value=$"BalanceEnquiry" },
                    new CardAction(){ Title = "Last 5 Transactions", Type=ActionTypes.PostBack, Value=$"Last5Transactions" },
                     new CardAction(){ Title = "Statement & Check book Request", Type=ActionTypes.PostBack, Value=$"StatementCheckbookRequest" },
                      new CardAction(){ Title = "Loan Account Information", Type=ActionTypes.PostBack, Value=$"LoanAccountInformation" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(ResumePhoneOptionsAsync);
        }
        public virtual async Task ResumePhoneOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var selection = await argument;
            if (selection.Text.Contains("BalanceEnquiry"))
            {
                string message = "Your account balance in your savings bank is AED: 25500";
                await context.PostAsync(message);

                var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(AnythingElseHandler);
            }
            else if (selection.Text.Contains("Last5Transactions"))
            {
                await context.PostAsync($@" {Environment.NewLine}Last 5 transactions:
                                     {Environment.NewLine}Card Number:2568-2321-628-2212
                                    {Environment.NewLine}Carrefour : 1500,
                                    {Environment.NewLine}Max fashion: 2500
                                    {Environment.NewLine}Geant: 3500");

                var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(AnythingElseHandler);
            }
            else if (selection.Text.Contains("StatementCheckbookRequest"))
            {
                //PromptDialog.Choice(context, ResumeStCheckbookOptionsAsync, 
                //    new List<string>()
                //    {
                //        "Statement",
                //        "Check book Request" }, 
                //    "Please select the desired service?");

                var feedback = ((Activity)context.Activity).CreateReply("Please select the desired service?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Statement", Type=ActionTypes.PostBack, Value=$"Statement" },
                    new CardAction(){ Title = "Check book Request", Type=ActionTypes.PostBack, Value=$"CheckbookRequest" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(ResumeStCheckbookOptionsAsync);

            }
            else if (selection.Text.Contains("LoanAccountInformation"))
            {

            }
        }
        public virtual async Task ResumeStCheckbookOptionsAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var selection = await argument;
            if (selection.Text.Contains("Statement"))
            {
                string message = "We will send you the bank statement to your registered email id.";
                await context.PostAsync(message);

                //CRMService.CreateTaskforStatement();

                var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(AnythingElseHandler);
            }
            else if (selection.Text.Contains("CheckbookRequest"))
            {
                string message = "We have created a service request for your check book request and will send you the cheque book to your registered address.";
                await context.PostAsync(message);

                //CRMService.CreateCaseforChequeBook();

                var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

                feedback.SuggestedActions = new SuggestedActions()
                {
                    Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
                };

                await context.PostAsync(feedback);

                context.Wait(AnythingElseHandler);
            }
        }


        public async Task CustomerNameFromGreeting(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            accountNumber = response;

            string message = "Thanks Sekhar for validating your account number.Tell me. How can i assist you?";
            await context.PostAsync(message);
        }
        public virtual async Task CustomerName(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            complaint = response;

            PromptDialog.Text(
            context: context,
            resume: CustomerEmailHandler,
            prompt: "May i know your name please?",
            retry: "Sorry, I don't understand that.");
        }

        public virtual async Task CustomerNameHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            customerName = response;

            PromptDialog.Text(
            context: context,
            resume: CustomerEmailHandler,
            prompt: "What is the best number to contact you?",
            retry: "Sorry, I don't understand that.");
        }
        public virtual async Task CustomerEmailHandler(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;
            phone = response;

            PromptDialog.Text(
            context: context,
            resume: FinalResultHandler,
            prompt: "What is your email id?",
            retry: "Sorry, I don't understand that.");
        }
        public virtual async Task FinalResultHandler(IDialogContext context, IAwaitable<string> argument)
        {
            string response = await argument;
            email = response;



            await context.PostAsync($@"Thank you for your interest, your request has been logged. Our customer service team will get back to you shortly.
                                    {Environment.NewLine}Your service request  summary:
                                    {Environment.NewLine}Complaint Title: {complaint},
                                    {Environment.NewLine}Customer Name: {customerName},
                                    {Environment.NewLine}Phone Number: {phone},
                                    {Environment.NewLine}Email: {email}");
            //string refno = CRMService.CreateCaseRegistration(complaint, customerName, phone, email);
            var feedback = ((Activity)context.Activity).CreateReply("Is there anything else that I could help?");

            feedback.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    //new CardAction(){ Title = "👍", Type=ActionTypes.PostBack, Value=$"yes-positive-feedback" },
                    //new CardAction(){ Title = "👎", Type=ActionTypes.PostBack, Value=$"no-negative-feedback" }

                     new CardAction(){ Title = "Yes", Type=ActionTypes.PostBack, Value=$"Yes" },
                    new CardAction(){ Title = "No", Type=ActionTypes.PostBack, Value=$"No" }
                }
            };

            await context.PostAsync(feedback);

            context.Wait(AnythingElseHandler);
        }
        public async Task AnythingElseHandler(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var answer = await argument;
            if (answer.Text.Contains("Yes"))
            {
                await GeneralGreeting(context, null);
            }
            else
            {
                string message = $"Thanks for using I Bot. Hope you have a great day!";
                await context.PostAsync(message);

                //var survey = context.MakeMessage();

                //var attachment = GetSurveyCard();
                //survey.Attachments.Add(attachment);

                //await context.PostAsync(survey);

                context.Done<string>("conversation ended.");
            }
        }
        public virtual async Task GeneralGreeting(IDialogContext context, IAwaitable<string> argument)
        {
            string message = $"Great! What else that can I help you?";
            await context.PostAsync(message);
            context.Wait(MessageReceivedAsync);
        }
        private async Task Escalate(Activity activity)
        {
            // This is our reference to the upstream conversation
            conversationRef = JsonConvert.SerializeObject(activity.ToConversationReference());

            var chatSpec = new ChatSpec()
            {
                // Set Agent skill to target
                Skill = "BotEscalation",
                VisitorName = activity.From.Name
            };

            // Start timer to poll for Live Assist chat events
            if (timer == null)
            {
                timer = timer ?? new Timer(5000);
                // OnTimedEvent is implemented in the next step
                timer.Elapsed += (sender, e) => OnTimedEvent(sender, e);
                timer.Start();
            }

            // Request a chat via the Sdk    
            chatContext = await sdk.RequestChat(chatSpec);
        }

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

        async void OnTimedEvent(Object source, ElapsedEventArgs eea)
        {
            if (chatContext != null)
            {
                // Create an upstream reply
                var reply = JsonConvert.DeserializeObject<ConversationReference>(conversationRef)
                    .GetPostToBotMessage().CreateReply();

                // Create upstream connection on which to send reply 
                var client = new ConnectorClient(new Uri(reply.ServiceUrl));

                // Poll Live Assist for events
                var chatInfo = await sdk.Poll(chatContext);

                if (chatInfo != null)
                {
                    // ChatInfo.ChatEvents will contain events since last call to poll.
                    if (chatInfo.ChatEvents != null && chatInfo.ChatEvents.Count > 0)
                    {
                        foreach (ChatEvent e in chatInfo.ChatEvents)
                        {
                            switch (e.Type)
                            {
                                // type is either "state" or "line".
                                case "line":
                                    // Source is either: "system", "agent" or "visitor"
                                    if (e.Source.Equals("system"))
                                    {
                                        reply.From.Name = "system";
                                    }
                                    else if (e.Source.Equals("agent"))
                                    {
                                        reply.From.Name = chatInfo.AgentName;

                                    }
                                    else
                                    {
                                        break;
                                    }

                                    reply.Type = "message";
                                    reply.Text = e.Text;
                                    client.Conversations.ReplyToActivity(reply);
                                    break;

                                case "state":
                                    // State changes
                                    // Valid values: "waiting", "chatting", "ended"
                                    if (chatInfo.State.Equals("ended"))
                                    {
                                        chatContext = null;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
        }
        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
        #endregion


    }
}