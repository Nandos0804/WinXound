using System;



namespace WinXound_Net
{
    class wxPosition : System.Collections.ArrayList
    {

        private const int mCAPACITY = 20;
        private int mIndex = 0;
        private int oldPos = -1;

        public wxPosition()
        {
            this.Capacity = mCAPACITY;
        }

        public void StoreCursorPos(Int32 position)
        {
            try
            {
                Int32 newPos = position;

                if (newPos != oldPos)
                {
                    this.AddValue(newPos);
                    oldPos = newPos;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxPosition - StoreCursorPos: " + ex.Message);
            }
        }


        public int PreviousPos()
        {
            try
            {
                if (this.Count == 0) return 0;

                mIndex++;
                if (mIndex >= this.Count) mIndex = this.Count - 1;
                if (mIndex >= mCAPACITY) mIndex = mCAPACITY - 1;

                return (int)this[mIndex];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxPosition - PreviousPos: " + ex.Message);
                return 0;
            }
        }

        public int NextPos()
        {
            try
            {
                if (this.Count == 0) return 0;

                mIndex--;
                if (mIndex < 0) mIndex = 0;

                return (int)this[mIndex];
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxPosition - NextPos: " + ex.Message);
                return 0;
            }
        }

        public void AddValue(Int32 Value)
        {
            try
            {
                if (this.Count <= mCAPACITY)
                {
                    this.Insert(0, Value);
                    mIndex = 0;
                }
                else
                {
                    this.RemoveAt(mCAPACITY);
                    this.Insert(0, Value);
                    mIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxPosition - AddValue: " + ex.Message);
            }
        }

        public Int32 Position()
        {
            return mIndex;
        }

        public void ClearAll()
        {
            try
            {
                this.Clear();
                mIndex = 0;
                oldPos = -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "wxPosition - ClearAll: " + ex.Message);
            }
        }



    }
}
