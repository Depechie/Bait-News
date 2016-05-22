using Foundation;
using System;
using UIKit;
using Softweb.Xamarin.Controls.iOS;
using CoreGraphics;
using DailyFail.Services;
using DailyFail.CustomControls;
using System.Collections.Generic;
using DailyFail.Models;
using System.Linq;
using System.Threading.Tasks;
using NotificationHub;
using SafariServices;

namespace DailyFail
{
    public partial class SwipeGameViewController : UIViewController, ICardViewDataSource
    {
        CardView HeadLineCardView { get; set; }
        IHeadlineService headlineService;
        List<Headline> headlines;
        Notifier incorrectHub;
        Notifier correctHub;
        List<Answer> answers;

        const string segueIdentifier = "RESULTS_SEGUE_IDENTIFIER";

        public SwipeGameViewController(IntPtr handle) : base(handle)
        {
            headlineService = new HeadlineService();
            answers = new List<Answer>();
        }

        async public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var result = await headlineService.GetHeadlines();
            headlines = result.ToList();

            if (HeadLineCardView == null)
            {
                HeadLineCardView = new CardView();
                HeadLineCardView.Center = new CGPoint(View.Center.X, View.Center.Y - 25);
                HeadLineCardView.Bounds = new CGRect(0f, 0f, (int)View.Bounds.Width - 40f, (int)View.Bounds.Height - 250f);

                HeadLineCardView.DidSwipeLeft += OnSwipeLeft;
                HeadLineCardView.DidSwipeRight += OnSwipeRight;

                HeadLineCardView.DataSource = this;

                View.AddSubview(HeadLineCardView);
            }

            incorrectHub = new Notifier(btnIncorrect);
            incorrectHub.MoveCircle(-48, -18);
            incorrectHub.SetCircleColor(btnIncorrect.TitleColor(UIControlState.Normal), UIColor.White);
            incorrectHub.ShowCount();

            correctHub = new Notifier(btnCorrect);
            correctHub.MoveCircle(-48, -18);
            correctHub.SetCircleColor(btnCorrect.TitleColor(UIControlState.Normal), UIColor.White);
            correctHub.ShowCount();
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == segueIdentifier)
            {
                var vc = (ResultsViewController)segue.DestinationViewController;
                if (vc == null)
                    return;

                vc.Answers = answers;
            }
        }

        async partial void BtnRead_TouchUpInside(UIButton sender)
        {
            var topCard = HeadLineCardView.Subviews.LastOrDefault();
            if (topCard != null)
            {
                var i = topCard as HeadlineView;
                //var safari = new SFSafariViewController(new NSUrl(topCard.Headline.Url), true);
                //await PresentViewControllerAsync(safari, true);
            }
        }

        partial void BtnFinish_TouchUpInside(UIButton sender)
        {
            //Could do something here..
        }

        public UIView NextCardForCardView(CardView cardView)
        {
            if (headlines.Count == 0)
            {
                DismissViewController(true, null);
                return new HeadlineView("Thats all folks")
                {
                    Frame = HeadLineCardView.Bounds,
                    BackgroundColor = UIColor.Clear
                };
            }

            var random = new Random();
            int index = random.Next(headlines.Count);
            var headline = headlines[index];
            headlines.RemoveAt(index);

            //Create a card with a random background color
            if (headline == null)
            {
                HeadLineCardView.RemoveFromSuperview();
                return new UIView();
            }

            var card = new HeadlineView(headline.Text)
            {
                Frame = HeadLineCardView.Bounds,
                BackgroundColor = UIColor.Clear,
                Headline = headline
            };

            card.Layer.ShouldRasterize = true;
            return card;
        }

        void OnSwipeLeft(object sender, SwipeEventArgs e)
        {
            var card = e.View as HeadlineView;
            var headline = card.Headline;

            var answer = new Answer() { Headline = headline };

            //User believes headline to be false
            if (headline.IsTrue)
            {
                incorrectHub.Increment(1, NotificationAnimationType.Pop);
                answer.CorrectAnswer = false;
            }
            else
            {
                correctHub.Increment(1, NotificationAnimationType.Pop);
                answer.CorrectAnswer = true;
            }
            answers.Add(answer);

        }

        void OnSwipeRight(object sender, SwipeEventArgs e)
        {
            var card = e.View as HeadlineView;
            var headline = card.Headline;

            var answer = new Answer() { Headline = headline };

            //User believes headline to be true
            if (headline.IsTrue)
            {
                correctHub.Increment(1, NotificationAnimationType.Pop);
                answer.CorrectAnswer = true;
            }
            else
            {
                incorrectHub.Increment(1, NotificationAnimationType.Pop);
                answer.CorrectAnswer = false;
            }
            answers.Add(answer);

        }

    }
}