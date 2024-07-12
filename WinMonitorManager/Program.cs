using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace MonitorControlApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer sleepTimer;
        private System.Windows.Forms.Timer wakeTimer;
        private System.Windows.Forms.Timer updateTimer;
        private DateTime nextSleepTime;
        private bool isRunning;
        private bool isPaused;

        public MainForm()
        {
            InitializeComponent();

            // Load settings
            //sleepTimeBox.Text = Properties.Settings.Default.SleepTime.ToString();
            //wakeTimeBox.Text = Properties.Settings.Default.WakeTime.ToString();
            //cycleCheckBox.Checked = Properties.Settings.Default.Cycle;
            //autoStartCheckBox.Checked = Properties.Settings.Default.AutoStart;

            sleepTimer = new System.Windows.Forms.Timer();
            wakeTimer = new System.Windows.Forms.Timer();
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000; // 1 second
            updateTimer.Tick += UpdateTimer_Tick;

            sleepTimer.Tick += SleepTimer_Tick;
            wakeTimer.Tick += WakeTimer_Tick;

            if (autoStartCheckBox.Checked)
            {
                StartTimers();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "MonitorControlApp";
            this.ClientSize = new System.Drawing.Size(300, 250);

            Label sleepLabel = new Label() { Text = "Таймер отключения:", Left = 10, Top = 10, Width = 150 };
            sleepTimeBox = new TextBox() { Left = 160, Top = 10, Width = 100 };

            Label wakeLabel = new Label() { Text = "Таймер сна:", Left = 10, Top = 40, Width = 150 };
            wakeTimeBox = new TextBox() { Left = 160, Top = 40, Width = 100 };

            cycleCheckBox = new CheckBox() { Text = "Зациклить задачу отключения", Left = 10, Top = 70, Width = 200 };
            autoStartCheckBox = new CheckBox() { Text = "Автозапуск таймера", Left = 10, Top = 100, Width = 200 };

            startButton = new Button() { Text = "Start", Left = 10, Top = 130, Width = 100 };
            startButton.Click += StartButton_Click;

            pauseButton = new Button() { Text = "Pause", Left = 120, Top = 130, Width = 100 };
            pauseButton.Click += PauseButton_Click;

            infoLabel = new Label() { Text = "Информация", Left = 10, Top = 160, Width = 250 };
            createByLabel = new Label() { Text = "Create by airmagicty, 2024", Left = 10, Top = 190, Width = 250 };

            this.Controls.Add(sleepLabel);
            this.Controls.Add(sleepTimeBox);
            this.Controls.Add(wakeLabel);
            this.Controls.Add(wakeTimeBox);
            this.Controls.Add(cycleCheckBox);
            this.Controls.Add(autoStartCheckBox);
            this.Controls.Add(startButton);
            this.Controls.Add(pauseButton);
            this.Controls.Add(infoLabel);
            this.Controls.Add(createByLabel);

            // Notify icon
            notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Monitor Control";
            notifyIcon.Icon = new System.Drawing.Icon("src/notify.ico"); // Укажите путь к вашей иконке
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            this.Resize += MainForm_Resize;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (isRunning)
            {
                StopTimers();
            }
            else
            {
                StartTimers();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                ResumeTimers();
            }
            else
            {
                PauseTimers();
            }
        }

        private void StartTimers()
        {
            int sleepTime = int.Parse(sleepTimeBox.Text) * 1000;
            int wakeTime = int.Parse(wakeTimeBox.Text) * 1000;

            sleepTimer.Interval = sleepTime;
            wakeTimer.Interval = sleepTime + wakeTime;
            nextSleepTime = DateTime.Now.AddMilliseconds(sleepTime);

            sleepTimer.Start();
            wakeTimer.Start();
            updateTimer.Start();

            isRunning = true;
            startButton.Text = "Stop";
            infoLabel.Text = $"До отключения: {sleepTimeBox.Text} сек.";

            sleepTimeBox.Enabled = false;
            wakeTimeBox.Enabled = false;

            // Save settings
            //Properties.Settings.Default.SleepTime = int.Parse(sleepTimeBox.Text);
            //Properties.Settings.Default.WakeTime = int.Parse(wakeTimeBox.Text);
            //Properties.Settings.Default.Cycle = cycleCheckBox.Checked;
            //Properties.Settings.Default.AutoStart = autoStartCheckBox.Checked;
            //Properties.Settings.Default.Save();
        }

        private void StopTimers()
        {
            sleepTimer.Stop();
            wakeTimer.Stop();
            updateTimer.Stop();

            isRunning = false;
            startButton.Text = "Start";
            infoLabel.Text = "Информация";

            sleepTimeBox.Enabled = true;
            wakeTimeBox.Enabled = true;
        }

        private void PauseTimers()
        {
            sleepTimer.Stop();
            wakeTimer.Stop();
            updateTimer.Stop();

            isPaused = true;
            pauseButton.Text = "Play";
        }

        private void ResumeTimers()
        {
            sleepTimer.Start();
            wakeTimer.Start();
            updateTimer.Start();

            isPaused = false;
            pauseButton.Text = "Pause";
        }

        private void SleepTimer_Tick(object sender, EventArgs e)
        {
            MonitorControl.SleepMonitor();
            sleepTimer.Stop();
        }

        private void WakeTimer_Tick(object sender, EventArgs e)
        {
            MonitorControl.WakeMonitor();
            wakeTimer.Stop();

            if (cycleCheckBox.Checked)
            {
                StartTimers();
            }
            else
            {
                StopTimers();
            }
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var remainingTime = nextSleepTime - DateTime.Now;
            if (remainingTime.TotalSeconds > 0)
            {
                infoLabel.Text = $"До отключения: {remainingTime.TotalSeconds:F0} сек.";
            }
            else
            {
                infoLabel.Text = "Монитор выключен";
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private TextBox sleepTimeBox;
        private TextBox wakeTimeBox;
        private CheckBox cycleCheckBox;
        private CheckBox autoStartCheckBox;
        private Button startButton;
        private Button pauseButton;
        private Label infoLabel;
        private Label createByLabel;
        private NotifyIcon notifyIcon;
    }

    static class MonitorControl
    {
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MONITORPOWER = 0xF170;

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static void SleepMonitor()
        {
            SendMessage(Form.ActiveForm.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)(2));
        }

        public static void WakeMonitor()
        {
            SendMessage(Form.ActiveForm.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)(-1));
        }
    }
}
