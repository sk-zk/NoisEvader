namespace NoisEvader.UI
{
    public partial class MessageBox
    {
        public MessageBox(string message)
        {
            BuildUI();
            Message.Text = message;
            Ok.Click += (_, __) => Close();
        }
    }
}