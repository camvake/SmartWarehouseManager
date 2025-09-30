using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

// Добавляем псевдоним для Timer
using Timer = System.Windows.Forms.Timer;

namespace SWM.Core.Services
{
    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public static class NotificationManager
    {
        private static List<NotificationForm> _activeNotifications = new List<NotificationForm>();
        private const int NOTIFICATION_WIDTH = 380;
        private const int NOTIFICATION_HEIGHT = 80;
        private const int SPACING = 10;
        private const int START_Y = 20;

        public static void Show(string message, NotificationType type = NotificationType.Info, int duration = 4000)
        {
            try
            {
                var notification = new NotificationForm(message, type, duration);
                _activeNotifications.Add(notification);

                PositionNotification(notification);
                notification.ShowNotification();

                notification.FormClosed += (s, e) => _activeNotifications.Remove(notification);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не падаем
                System.Diagnostics.Debug.WriteLine($"Notification error: {ex.Message}");
            }
        }

        public static void ShowSuccess(string message, int duration = 3000)
        {
            Show(message, NotificationType.Success, duration);
        }

        public static void ShowError(string message, int duration = 5000)
        {
            Show(message, NotificationType.Error, duration);
        }

        public static void ShowWarning(string message, int duration = 4000)
        {
            Show(message, NotificationType.Warning, duration);
        }

        public static void ShowInfo(string message, int duration = 4000)
        {
            Show(message, NotificationType.Info, duration);
        }

        private static void PositionNotification(NotificationForm notification)
        {
            try
            {
                var screen = Screen.PrimaryScreen.WorkingArea;
                int x = screen.Right - NOTIFICATION_WIDTH - SPACING;
                int y = START_Y;

                // Сдвигаем вниз для каждого нового уведомления
                foreach (var activeNotif in _activeNotifications)
                {
                    if (activeNotif != notification && activeNotif.Visible && !activeNotif.IsDisposed)
                    {
                        y += NOTIFICATION_HEIGHT + SPACING;
                    }
                }

                notification.Location = new Point(x, y);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Position error: {ex.Message}");
            }
        }

        public static void CloseAll()
        {
            foreach (var notification in _activeNotifications.ToArray())
            {
                try
                {
                    if (!notification.IsDisposed)
                        notification.CloseNotification();
                }
                catch { }
            }
            _activeNotifications.Clear();
        }
    }

    public class NotificationForm : Form
    {
        private string _message;
        private NotificationType _type;
        private int _duration;

        private Timer _closeTimer;
        private Timer _fadeTimer;
        private Timer _progressTimer;
        private bool _isClosing = false;
        private bool _isPaused = false;

        private Label _lblIcon;
        private Label _lblMessage;
        private Button _btnClose;
        private Panel _progressBar;
        private Panel _mainPanel;

        public NotificationForm(string message, NotificationType type, int duration)
        {
            _message = message;
            _type = type;
            _duration = duration;

            // Создаем handle сразу
            this.CreateHandle();
            InitializeForm();
        }

        private void InitializeForm()
        {
            if (this.IsDisposed) return;

            this.SuspendLayout();

            // Основные настройки формы
            this.Size = new Size(380, 80);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Opacity = 0;
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);

            // Сначала создаем все контролы
            CreateControls();

            // Затем настраиваем внешний вид
            SetAppearance();
            SetIcon();

            // События для hover эффекта
            SetupHoverEvents();

            this.ResumeLayout(false);
        }

        private void CreateControls()
        {
            // Главная панель с тенью
            _mainPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(380, 80),
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Иконка
            _lblIcon = new Label()
            {
                Location = new Point(15, 20),
                Size = new Size(40, 40),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            // Сообщение
            _lblMessage = new Label()
            {
                Location = new Point(65, 15),
                Size = new Size(260, 50),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Text = _message,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            // Кнопка закрытия
            _btnClose = new Button()
            {
                Text = "×",
                Location = new Point(340, 8),
                Size = new Size(24, 24),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
            _btnClose.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 255, 255, 255);
            _btnClose.Click += (s, e) => CloseNotification();

            // Прогресс-бар
            _progressBar = new Panel()
            {
                Location = new Point(0, 75),
                Size = new Size(380, 5),
                BackColor = Color.White
            };

            // Добавляем элементы на главную панель
            _mainPanel.Controls.AddRange(new Control[] { _lblIcon, _lblMessage, _btnClose, _progressBar });

            // Добавляем главную панель на форму
            this.Controls.Add(_mainPanel);
        }

        private void SetAppearance()
        {
            if (_mainPanel == null || _progressBar == null) return;

            Color backgroundColor;
            Color progressColor;

            switch (_type)
            {
                case NotificationType.Success:
                    backgroundColor = Color.FromArgb(46, 125, 50); // Green
                    progressColor = Color.FromArgb(129, 199, 132); // Light Green
                    break;
                case NotificationType.Error:
                    backgroundColor = Color.FromArgb(211, 47, 47); // Red
                    progressColor = Color.FromArgb(229, 115, 115); // Light Red
                    break;
                case NotificationType.Warning:
                    backgroundColor = Color.FromArgb(237, 108, 2); // Orange
                    progressColor = Color.FromArgb(255, 167, 38); // Light Orange
                    break;
                case NotificationType.Info:
                    backgroundColor = Color.FromArgb(2, 119, 189); // Blue
                    progressColor = Color.FromArgb(100, 181, 246); // Light Blue
                    break;
                default:
                    backgroundColor = Color.FromArgb(2, 119, 189);
                    progressColor = Color.FromArgb(100, 181, 246);
                    break;
            }

            _mainPanel.BackColor = backgroundColor;
            _progressBar.BackColor = progressColor;
        }

        private void SetIcon()
        {
            if (_lblIcon == null) return;

            switch (_type)
            {
                case NotificationType.Success:
                    _lblIcon.Text = "✓";
                    break;
                case NotificationType.Error:
                    _lblIcon.Text = "✕";
                    break;
                case NotificationType.Warning:
                    _lblIcon.Text = "⚠";
                    break;
                case NotificationType.Info:
                    _lblIcon.Text = "ℹ";
                    break;
            }
        }

        private void SetupHoverEvents()
        {
            if (this.IsDisposed) return;

            // Для всей формы
            this.MouseEnter += (s, e) => PauseAutoClose();
            this.MouseLeave += (s, e) => ResumeAutoClose();

            // Для главной панели
            _mainPanel.MouseEnter += (s, e) => PauseAutoClose();
            _mainPanel.MouseLeave += (s, e) => ResumeAutoClose();

            // Для иконки
            _lblIcon.MouseEnter += (s, e) => PauseAutoClose();
            _lblIcon.MouseLeave += (s, e) => ResumeAutoClose();
            _lblIcon.Click += (s, e) => CloseNotification();

            // Для сообщения
            _lblMessage.MouseEnter += (s, e) => PauseAutoClose();
            _lblMessage.MouseLeave += (s, e) => ResumeAutoClose();
            _lblMessage.Click += (s, e) => CloseNotification();
        }

        public void ShowNotification()
        {
            if (this.IsDisposed || !this.IsHandleCreated) return;

            try
            {
                // Показываем форму
                this.Show();

                // Анимация появления
                FadeIn();

                // Запускаем таймер автоматического закрытия
                if (_duration > 0)
                {
                    StartAutoCloseTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Show notification error: {ex.Message}");
            }
        }

        private void FadeIn()
        {
            if (this.IsDisposed) return;

            _fadeTimer = new Timer();
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += (s, e) =>
            {
                if (this.IsDisposed)
                {
                    _fadeTimer?.Stop();
                    return;
                }

                if (this.Opacity < 0.95)
                {
                    this.Opacity += 0.15;
                }
                else
                {
                    this.Opacity = 0.95;
                    _fadeTimer?.Stop();
                    _fadeTimer?.Dispose();
                }
            };
            _fadeTimer.Start();
        }

        private void FadeOut()
        {
            if (_isClosing || this.IsDisposed) return;
            _isClosing = true;

            // Останавливаем все таймеры
            _closeTimer?.Stop();
            _progressTimer?.Stop();

            _fadeTimer = new Timer();
            _fadeTimer.Interval = 15;
            _fadeTimer.Tick += (s, e) =>
            {
                if (this.IsDisposed)
                {
                    _fadeTimer?.Stop();
                    return;
                }

                if (this.Opacity > 0.1)
                {
                    this.Opacity -= 0.15;
                }
                else
                {
                    this.Opacity = 0;
                    _fadeTimer?.Stop();
                    _fadeTimer?.Dispose();
                    try
                    {
                        if (!this.IsDisposed)
                        {
                            this.Close();
                            this.Dispose();
                        }
                    }
                    catch { }
                }
            };
            _fadeTimer.Start();
        }

        private void StartAutoCloseTimer()
        {
            if (this.IsDisposed) return;

            _closeTimer = new Timer();
            _closeTimer.Interval = _duration;
            _closeTimer.Tick += (s, e) => CloseNotification();
            _closeTimer.Start();

            // Анимация прогресс-бара
            StartProgressBarAnimation();
        }

        private void StartProgressBarAnimation()
        {
            if (this.IsDisposed || _progressBar == null) return;

            _progressTimer = new Timer();
            _progressTimer.Interval = 50;
            int totalSteps = _duration / _progressTimer.Interval;
            int currentStep = 0;

            _progressTimer.Tick += (s, e) =>
            {
                if (_isClosing || _isPaused || this.IsDisposed || _progressBar.IsDisposed)
                {
                    _progressTimer?.Stop();
                    return;
                }

                currentStep++;
                int newWidth = (int)((double)currentStep / totalSteps * 380);
                _progressBar.Width = 380 - newWidth;

                if (currentStep >= totalSteps)
                {
                    _progressTimer?.Stop();
                }
            };
            _progressTimer.Start();
        }

        private void PauseAutoClose()
        {
            if (_isClosing || this.IsDisposed || _progressBar == null) return;

            _isPaused = true;
            _closeTimer?.Stop();
            _progressTimer?.Stop();

            // Затемняем прогресс-бар при паузе
            if (!_progressBar.IsDisposed)
                _progressBar.BackColor = Color.FromArgb(150, _progressBar.BackColor);
        }

        private void ResumeAutoClose()
        {
            if (_isClosing || this.IsDisposed) return;

            _isPaused = false;
            _closeTimer?.Start();

            // Восстанавливаем цвет прогресс-бара
            SetAppearance();

            _progressTimer?.Start();
        }

        public void CloseNotification()
        {
            if (_isClosing || this.IsDisposed) return;

            _closeTimer?.Stop();
            _progressTimer?.Stop();

            FadeOut();
        }

        protected override void OnDeactivate(EventArgs e)
        {
            if (!this.IsDisposed)
            {
                // Не позволяем форме терять фокус
                this.TopMost = true;
                base.OnDeactivate(e);
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (!this.IsDisposed)
                {
                    // Добавляем стиль окна, который предотвращает активацию
                    cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                    cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                }
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.IsDisposed)
            {
                base.OnPaint(e);

                // Рисуем тень вокруг формы
                using (var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
        }

        // Публичные методы для управления уведомлением
        public void UpdateMessage(string newMessage)
        {
            if (this.IsDisposed || _lblMessage == null) return;

            if (_lblMessage.InvokeRequired)
            {
                _lblMessage.Invoke(new Action<string>(UpdateMessage), newMessage);
            }
            else
            {
                _lblMessage.Text = newMessage;
            }
        }

        public void UpdateType(NotificationType newType)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<NotificationType>(UpdateType), newType);
            }
            else
            {
                _type = newType;
                SetAppearance();
                SetIcon();
            }
        }

        public void ExtendDuration(int additionalMilliseconds)
        {
            if (_closeTimer != null && !_isClosing && !this.IsDisposed)
            {
                _closeTimer.Stop();
                _duration += additionalMilliseconds;
                _closeTimer.Interval = _duration;
                _closeTimer.Start();

                // Перезапускаем анимацию прогресс-бара
                _progressTimer?.Stop();
                StartProgressBarAnimation();
            }
        }

        protected override void Dispose(bool disposing)
        {
            _closeTimer?.Stop();
            _fadeTimer?.Stop();
            _progressTimer?.Stop();

            _closeTimer?.Dispose();
            _fadeTimer?.Dispose();
            _progressTimer?.Dispose();

            base.Dispose(disposing);
        }
    }
}