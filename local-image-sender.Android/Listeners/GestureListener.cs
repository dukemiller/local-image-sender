using System;
using Android.Content;
using Android.Views;
using Exception = Java.Lang.Exception;

namespace local_image_sender.Android.Listeners
{
    // https://stackoverflow.com/questions/4139288/android-how-to-handle-right-to-left-swipe-gestures
    public class GestureListener : GestureDetector.SimpleOnGestureListener, View.IOnTouchListener
    {
        private readonly GestureDetector _gestureDetector;

        public GestureListener(Context ctx) => _gestureDetector = new GestureDetector(ctx, this);

        public bool OnTouch(View v, MotionEvent e) => _gestureDetector.OnTouchEvent(e);

        private const int SwipeThreshold = 100;

        private const int SwipeVelocityThreshold = 100;

        public override bool OnDown(MotionEvent e) => true;

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            var result = false;
            try
            {
                var diffY = e2.GetY() - e1.GetY();
                var diffX = e2.GetX() - e1.GetX();

                if (Math.Abs(diffX) > Math.Abs(diffY))
                {
                    if (Math.Abs(diffX) > SwipeThreshold && Math.Abs(velocityX) > SwipeVelocityThreshold)
                    {
                        if (diffX > 0)
                            OnSwipeRight();
                        else
                            OnSwipeLeft();

                        result = true;
                    }
                }

                else if (Math.Abs(diffY) > SwipeThreshold && Math.Abs(velocityY) > SwipeVelocityThreshold)
                {
                    if (diffY > 0)
                        OnSwipeBottom();
                    else
                        OnSwipeTop();
                    result = true;
                }
            }

            catch (Exception)
            {
            }

            return result;
        }

        public Action OnSwipeRight { get; set; } = () => { };

        public Action OnSwipeLeft { get; set; } = () => { };

        public Action OnSwipeTop { get; set; } = () => { };

        public Action OnSwipeBottom { get; set; } = () => { };
    }
}