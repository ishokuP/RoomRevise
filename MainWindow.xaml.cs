
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
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
        private const string CsvFilePath = "E:/Code/RoomRevise/sample3.csv";
        private static List<EventData>? eventsData;
        private DispatcherTimer dispatcherTimer;
        public MainWindow()
        {
            LoadCSV(CsvFilePath);

            InitializeComponent();

            CurrTime.Content = DateTime.Now.ToString("HH:mm ss tt");
            CurrDate.Content = DateTime.Now.ToString("MMMM dd, dddd");

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            CurrTime.Content = DateTime.Now.ToString("HH:mm ss tt");
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
                .Where(eventData => now.TimeOfDay <= eventData.StartTime)
                .OrderBy(eventData => eventData.StartTime)
                .Take(3);


            bool Use12HourFormat = true;
            int upcNumber = 1;

            if (currentEvent != null)
            {
                CurrEventName.Content = currentEvent.EventName;
                CurrEventTime.Content = Use12HourFormat
                    ? $"{ConvertTo12HourFormat(currentEvent.StartTime)} - {ConvertTo12HourFormat(currentEvent.EndTime)}"
                    : $"{currentEvent.StartTime:g} - {currentEvent.EndTime:g}";

                foreach (var eventItem in upcomingEvent)
                {
                    Label upcgName = FindName($"UpcgName{upcNumber}") as Label;
                    Label upcgTime = FindName($"UpcgTime{upcNumber}") as Label;

                    if (upcgName != null && upcgTime != null)
                    {
                        upcgName.Content = eventItem.EventName;
                        upcgTime.Content = Use12HourFormat
                            ? $"{ConvertTo12HourFormat(eventItem.StartTime)} - {ConvertTo12HourFormat(eventItem.EndTime)}"
                            : $"{eventItem.StartTime:g} to {eventItem.EndTime:g}";
                        upcgName.Visibility = Visibility.Visible;
                        upcgTime.Visibility = Visibility.Visible;
                    }

                    upcNumber++;
                }

                // Collapse labels if there are no further upcoming events
                for (int i = upcNumber; i <= 3; i++)
                {
                    Label upcgName = FindName($"UpcgName{i}") as Label;
                    Label upcgTime = FindName($"UpcgTime{i}") as Label;

                    if (upcgName != null && upcgTime != null)
                    {
                        upcgName.Visibility = Visibility.Collapsed;
                        upcgTime.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                // If no current event, clear the UI elements and collapse all labels
                CurrEventName.Content = "Free Time";
                CurrEventTime.Content = "No Scheduled Event!";

                for (int i = 1; i <= 3; i++)
                {
                    Label upcgName = FindName($"UpcgName{i}") as Label;
                    Label upcgTime = FindName($"UpcgTime{i}") as Label;

                    if (upcgName != null && upcgTime != null)
                    {
                        upcgName.Visibility = Visibility.Collapsed;
                        upcgTime.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private string ConvertTo12HourFormat(TimeSpan time)
        {
            return DateTime.Today.Add(time).ToString("hh:mm tt");
        }

        private void openImport(object sender, RoutedEventArgs e)
        {
            var filePath = String.Empty;
            var dialog = new OpenFileDialog();

            dialog.Multiselect = false;
            dialog.Filter = "CSV Files (*.csv)|*.csv|XML Files(*.xml)|*.xml";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            bool? results = dialog.ShowDialog();

            if (results == true)
            {
                string dialogFile = dialog.FileName;
                IFileImporter importer;
                if (dialogFile.EndsWith(".csv"))
                {
                    importer = new CSVFileImporter();
                } else if (dialogFile.EndsWith(".xml"))
                {
                    importer = new XMLFileImporter();

                } else
                {
                    MessageBox.Show("Unsupported file type.");
                    return;
                }

                List<EventData> eventDataList = importer.Import(filePath);
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


    public interface IFileImporter
    {
        List<EventData> Import(string filePath);
    }

    // CSV file importer
    public class CSVFileImporter : IFileImporter
    {
        public List<EventData> Import(string filePath)
        {

            List<EventData> eventDataList = new List<EventData>();

            return eventDataList;
        }
    }

    // XML file importer
    public class XMLFileImporter : IFileImporter
    {
        public List<EventData> Import(string filePath)
        {

            List<EventData> eventDataList = new List<EventData>();

            return eventDataList;
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

