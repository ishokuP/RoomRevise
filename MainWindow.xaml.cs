
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using System.IO;
using RoomRevise;
using System.Windows.Media.Imaging;
using System.Windows.Media;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string CsvFilePath = "currentSchedule.csv";
        private bool Use12HourFormat = true;

        private static List<EventData>? eventsData;
        private DispatcherTimer dispatcherTimer;
        public MainWindow()
        {
            if (!File.Exists(CsvFilePath))
            {
                firstTimeCSV csvCreator = new firstTimeCSV();
                csvCreator.CreateSampleCSV();
            }

            InitializeComponent();
            LoadCSV(CsvFilePath);


            CurrTime.Content = DateTime.Now.ToString("HH:mm tt");
            CurrDate.Content = DateTime.Now.ToString("MMMM dd, dddd");

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();

        }
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            Use12hourformat.IsChecked = Use12HourFormat;

            CurrTime.Content = Use12HourFormat
                ? DateTime.Now.ToString("hh:mm tt")
                : DateTime.Now.ToString("HH:mm ");

            CurrDate.Content = DateTime.Now.ToString("MMMM dd, dddd");

            DateTime now = DateTime.Now;
            DayOfWeek currentDay = now.DayOfWeek;

            // Filter events for the current day
            var todayEvents = eventsData?
                .Where(eventData => eventData.DayOfWeek == currentDay)
                .ToList();

            // Find the first event that is currently happening
            var currentEvent = todayEvents?.FirstOrDefault(eventData =>
                now.TimeOfDay >= eventData.StartTime && now.TimeOfDay <= eventData.EndTime);

            var upcomingEvent = todayEvents?
                .Where(eventData => now.TimeOfDay <= eventData.StartTime)
                .OrderBy(eventData => eventData.StartTime)
                .Take(3);

            int upcNumber = 1;
            if (currentEvent != null)
            {
                // Display current event details
                CurrEventName.Content = currentEvent.EventName;
                CurrEventTime.Content = Use12HourFormat
                    ? $"{ConvertTo12HourFormat(currentEvent.StartTime)} - {ConvertTo12HourFormat(currentEvent.EndTime)}"
                    : $"{currentEvent.StartTime:g} - {currentEvent.EndTime:g}";
                if (upcomingEvent != null)
                {
                    // Populate and display upcoming events
                    foreach (var eventItem in upcomingEvent)
                    {
                        Label? upcgName = FindName($"UpcgName{upcNumber}") as Label;
                        Label? upcgTime = FindName($"UpcgTime{upcNumber}") as Label;

                        if (upcgName != null && upcgTime != null)
                        {
                            upcgName.Content = eventItem.EventName;
                            upcgTime.Content = Use12HourFormat
                                ? $"{ConvertTo12HourFormat(eventItem.StartTime)} - {ConvertTo12HourFormat(eventItem.EndTime)}"
                                : $"{eventItem.StartTime:g} - {eventItem.EndTime:g}";
                            upcgName.Visibility = Visibility.Visible;
                            upcgTime.Visibility = Visibility.Visible;
                        }

                        upcNumber++;
                    }
                }
               

                // Collapse labels if there are no further upcoming events
                for (int i = upcNumber; i <= 3; i++)
                {
                    Label? upcgName = FindName($"UpcgName{i}") as Label;
                    Label? upcgTime = FindName($"UpcgTime{i}") as Label;

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

                // Populate and display upcoming events
                if (upcomingEvent != null)
                {
                    foreach (var eventItem in upcomingEvent)
                    {
                        Label? upcgName = FindName($"UpcgName{upcNumber}") as Label;
                        Label? upcgTime = FindName($"UpcgTime{upcNumber}") as Label;

                        if (upcgName != null && upcgTime != null && eventItem != null) // Check eventItem for null
                        {
                            upcgName.Content = eventItem.EventName; // No null-conditional operator needed
                            upcgTime.Content = Use12HourFormat
                                ? $"{ConvertTo12HourFormat(eventItem.StartTime)} - {ConvertTo12HourFormat(eventItem.EndTime)}"
                                : $"{eventItem.StartTime:g} - {eventItem.EndTime:g}";
                            upcgName.Visibility = Visibility.Visible;
                            upcgTime.Visibility = Visibility.Visible;
                        }

                        upcNumber++;
                    }
                }


                // Collapse labels if there are no further upcoming events
                for (int i = upcNumber; i <= 3; i++)
                {
                    Label? upcgName = FindName($"UpcgName{i}") as Label;
                    Label? upcgTime = FindName($"UpcgTime{i}") as Label;

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
                }
                else if (dialogFile.EndsWith(".xml"))
                {
                    importer = new XMLFileImporter();
                }
                else
                {
                    MessageBox.Show("Unsupported file type.");
                    return;
                }

                List<EventData> eventDataList = importer.Import(dialogFile);

                // Update the existing CSV file with the imported data
                File.WriteAllLines(CsvFilePath, eventDataList.Select(eventData =>
                    $"{eventData.DayOfWeek},{eventData.StartTime},{eventData.EndTime},{eventData.EventName}"));

                // Reload the CSV data
                LoadCSV(CsvFilePath);
            }
        }

        static void LoadCSV(string filePath)
        {
            eventsData = new List<EventData>();
            eventsData?.Clear();
            try
            {

                using TextFieldParser parser = new(filePath);

                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();


                    if (fields != null)
                    {
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
                            if (eventsData != null)
                            {
                                eventsData.Add(eventData);

                            }

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw new ArgumentException($"File does not {ex.Message} ");
            }
        }
        private void twelvehourtoggle(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                Use12HourFormat = menuItem.IsChecked;
            }
        }





        private void ChangeBackground(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                var imagePath = openFileDialog.FileName;
                var bitmapImage = new BitmapImage(new Uri(imagePath));
                bgStack.Background = new ImageBrush(bitmapImage);
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

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Skip the header line
                    parser.ReadLine();

                    while (!parser.EndOfData)
                    {
                        string[]? fields = parser.ReadFields();

                        if (fields != null && fields.Length >= 4 && Enum.TryParse(fields[0], true, out DayOfWeek dayOfWeek)
                            && TimeSpan.TryParse(fields[1], out TimeSpan startTime) && TimeSpan.TryParse(fields[2], out TimeSpan endTime))
                        {
                            var eventData = new EventData
                            {
                                DayOfWeek = dayOfWeek,
                                StartTime = startTime,
                                EndTime = endTime,
                                EventName = fields[3]
                            };

                            eventDataList.Add(eventData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return eventDataList;
        }
    }


    // XML file importer
    public class XMLFileImporter : IFileImporter
    {
        public List<EventData> Import(string filePath)
        {
            List<EventData> eventDataList = new List<EventData>();

            try
            {
                XDocument doc = XDocument.Load(filePath);

                // Assuming each event is represented by an "Event" element in the XML
                foreach (XElement element in doc.Descendants("Event"))
                {
                    // Parse XML attributes and populate EventData objects
                    DayOfWeek dayOfWeek;
                    Enum.TryParse(element.Attribute("DayOfWeek")?.Value, true, out dayOfWeek);

                    TimeSpan startTime;
                    TimeSpan.TryParse(element.Attribute("StartTime")?.Value, out startTime);

                    TimeSpan endTime;
                    TimeSpan.TryParse(element.Attribute("EndTime")?.Value, out endTime);

                    string eventName = element.Element("EventName")!.Value;

                    EventData eventData = new EventData
                    {
                        DayOfWeek = dayOfWeek,
                        StartTime = startTime,
                        EndTime = endTime,
                        EventName = eventName
                    };

                    eventDataList.Add(eventData);
                }

                // Update the existing XML file with the imported data
                doc.Descendants("Event").Remove(); // Clear existing events
                foreach (var eventData in eventDataList)
                {
                    XElement newEvent = new XElement("Event",
                        new XAttribute("DayOfWeek", eventData.DayOfWeek),
                        new XAttribute("StartTime", eventData.StartTime),
                        new XAttribute("EndTime", eventData.EndTime),
                        new XElement("EventName", eventData.EventName)
                    );
                    doc.Root?.Add(newEvent);
                }
                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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

