namespace Mrivai.Pelitabangsa
{
    /// <summary>
    /// manage device input event shell path
    /// </summary>
    public class InputEvent
    {
        private string shellpath;
        /// <summary>
        /// set input event shell path by name
        /// </summary>
        public InputEvent(string eventname)
        {
            shellpath = "/dev/input/" + eventname;

        }
        /// <summary>
        /// set input event shell path by number
        /// </summary>
        public InputEvent(int eventnumber)
        {
            shellpath = "/dev/input/event" + eventnumber;
        }
        /// <summary>
        /// get input event shell path
        /// </summary>
        public string getShellPath()
        {
            return shellpath;
        }
    }
}
