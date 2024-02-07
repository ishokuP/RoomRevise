﻿
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CsvFilePath = "E:/Code/RoomRevise/sample2.csv";
        private static List<EventData>? eventsData;
        private DispatcherTimer dispatcherTimer;
        public MainWindow()
        {
            LoadCSV(CsvFilePath);

            InitializeComponent();

            CurrTime.Content = DateTime.Now.ToString("hh:mm ss tt");
            CurrDate.Content = DateTime.Now.ToString("MMMM dd, dddd");

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();



        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            CurrTime.Content = DateTime.Now.ToString("hh:mm ss tt");
            CurrDate.Content = DateTime.Now.ToString("MMMM dd, dddd");

            DateTime now = DateTime.Now;
            DayOfWeek currentDay = now.DayOfWeek;

            // Filter events for the current day
            var todayEvents = eventsData
                .Where(eventData => eventData.DayOfWeek == currentDay)
                .ToList();

            // Find the first event that is currently happening
            var currentEvent = todayEvents.FirstOrDefault(eventData =>
                now.TimeOfDay >= eventData.StartTime && now.TimeOfDay <= eventData.EndTime);

            var upcomingEvent = todayEvents
                .Where(eventData => now.TimeOfDay < eventData.StartTime);

            if (currentEvent != null && upcomingEvent.Any())
            {
                CurrEventName.Content = currentEvent.EventName;
                CurrEventTime.Content = $"{currentEvent.StartTime:g} - {currentEvent.EndTime:g}";


            }
            else
            {
                // If no current event, clear the UI elements
                CurrEventName.Content = string.Empty;
                CurrEventTime.Content = string.Empty;
            }
        }

        static void LoadCSV(string filePath)
        {
            eventsData = new List<EventData>();

            try
            {

                using TextFieldParser parser = new(filePath);
                
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                while (!parser.EndOfData)
                    {
                    string[]? fields = parser.ReadFields();



                        if (fields.Length >= 4 && Enum.TryParse(fields[0], true, out DayOfWeek dayOfWeek)
                            && TimeSpan.TryParse(fields[1], out TimeSpan startTime) && TimeSpan.TryParse(fields[2], out TimeSpan endTime))
                        {
                            var eventData = new EventData
                            {
                                DayOfWeek = dayOfWeek,
                                StartTime = startTime,
                                EndTime = endTime,
                                EventName = fields[3]
                            };

                            eventsData.Add(eventData);
                        }
                    }
                
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"File does not {ex.Message} ");
            }
        }
    }
    public static class Miliseconds
    {
        public static DateTime TrimMilliseconds(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0, dt.Kind);
        }
    }

    public class EventData
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public required string EventName { get; set; }
    }


}

