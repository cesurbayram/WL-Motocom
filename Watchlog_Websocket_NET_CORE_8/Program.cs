using HSEthernet;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using static HSEthernet.HSEClient;
using Microsoft.VisualBasic.FileIO;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;
using Timer = System.Threading.Timer;
using System.Diagnostics;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;
using YMConnect;
using Watchlog_Websocket_NET_CORE_8.Classes;
using System.Security.Claims;
using System.Net;
using System.Collections;
using System.IO.Compression;
using Watchlog_Websocket_NET_CORE_8.Classes.DataAccess;
using Watchlog_Websocket_NET_CORE_8.Classes.Services;
using YMConnect.Interop;
using System.Dynamic;
using Watchlog_Websocket_NET_CORE_8.Classes.Entityies;

public class WebSocketMessage
{
    public string type { get; set; }
    public Data data { get; set; }
}
public class Data
{
    public string type { get; set; }
    public string ipAddress { get; set; }
    public List<Values> values { get; set; }
    public string controllerName { get; set; }
    public List<string> fileTypes { get; set; }
    public string requestId { get; set; }
    public string controllerId { get; set; }
}
public class Values
{
    public string JobName { get; set; } // Sadece "JobSelect" ve "Start" tipinde gelir


    public List<string> signalNumbers { get; set; }  // Sadece "Start" tipinde gelir
    public int duration { get; set; }  // Sadece "Start" tipinde gelir
}


class Program
{
    #region Sabitler ve Konfigürasyon
    private static class TaskDelays
    {
        public const int VARIABLE_DELAY = 1000;      // Değişkenler için   DEFAULT 1000
        public const int VARIABLE_STRING = 1000;      // Değişkenler için  DEFAULT 5000
        public const int STATUS_DELAY = 500;        // Robot durumu için  DEFAULT 650
        public const int IO_DELAY = 700;           // IO işlemleri için   DEFAULT 1000     
        public const int ALARM_DELAY = 400;       // Alarm kontrolleri için  DEFAULT 1000
        public const int ALMHIST_DELAY = 300000;     // Alarm geçmişi için  DEFAULT 300000    5 dk
        public const int WEBSOCKET_SEND_DELAY = 10; // WebSocket gönderimi için  DEFAULT 10
        public const int MONITOR_DELAY = 10000;     // Sistem monitör gecikmesi  DEFAULT 10000
        public const int GET_MANAGEMENT_TIME_DELAY = 600000; // 600000 10 dk da bir çekimesi okeydir.
        public const int JOB_DATA_DELAY = 700; // DEFAULT 700
        public const int TORK_DATA_DELAY = 5; // DEFAULT 150
        public const int ABSO_DATA_DELAY = 60000; // DEFAULT 60000

        public const int TORK_EXAM_DELAY = 5;// DEFAULT 150
        public const int TORK_EXAM_JOB_DELAY = 100;// DEFAULT 150
        public const int TORK_EXAM_SIGNAL_DELAY = 5;// DEFAULT 150

        public const int REGISTER_DELAY = 1000;

    }

    private static string WSMes_TorkExam_Type { get; set; }
    private static string WSMes_TorkExam_SelectJob { get; set; }
    private static List<string> WSMes_TorkExam_SignalNo { get; set; }
    private static string WSMes_TorkExam_JobName { get; set; }
    private static int WSMes_TorkExam_Time { get; set; }


    private static class QueueConfig
    {
        public const int MAX_QUEUE_SIZE = 1000;

        public static void EnqueueWithLimit<T>(ConcurrentQueue<T> queue, T item)
        {
            if (queue.Count < MAX_QUEUE_SIZE)
            {
                queue.Enqueue(item);
            }
        }
    }

    private static class RobotConnection
    {
        private static readonly ConcurrentDictionary<string, HSEClient> _robotClients = new ConcurrentDictionary<string, HSEClient>();
        private static readonly ConcurrentDictionary<string, bool> _lastConnectionStates = new ConcurrentDictionary<string, bool>();

        public static HSEClient GetClient(string ip, out bool Connect)
        {
            return new HSEClient(ip, out Connect);
        }

        public static async Task CheckAndLogConnectionState(string ip, HSEClient client)
        {
            client = new HSEClient("192.168.255.1", out bool RobotConnect);

            bool isCurrentlyConnected = RobotConnect;
            bool wasConnected = _lastConnectionStates.GetOrAdd(ip, false);

            if (isCurrentlyConnected != wasConnected)
            {
                if (isCurrentlyConnected)
                {
                    Console.WriteLine($"{DateTime.Now} Robot {ip} bağlantısı başarıyla kuruldu.");
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} Robot {ip} bağlantısı koptu!");
                }
                _lastConnectionStates[ip] = isCurrentlyConnected;
            }
        }
    }
    #endregion

    #region Kuyruklar ve Sayaçlar
    private static readonly ConcurrentQueue<string> _Variable_Byte_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _Variable_Integer_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _Variable_Double_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _Variable_Real_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _Variable_String_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _RobotStatus_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _Alarm_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _AlarmHist_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_General_Input_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_General_Output_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_External_Input_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_External_Output_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Network_Input_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Network_Output_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Specific_Input_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Specific_Output_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_AuxiliaryRelay_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Internal_Control_Status_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _IO_Pseudo_Input_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _GetManagementTime_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _JobData_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkData_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _AbsoData_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkExamJobList_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkExamJobSelect_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkExamTork_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkExamJob_Queue = new ConcurrentQueue<string>();
    private static readonly ConcurrentQueue<string> _TorkExamSignal_Queue = new ConcurrentQueue<string>();

    private static readonly ConcurrentQueue<string> _Register_Queue = new ConcurrentQueue<string>();



    private static int _Variable_Byte_FetchCount;
    private static int _Variable_Integer_FetchCount;
    private static int _Variable_Double_FetchCount;
    private static int _Variable_Real_FetchCount;
    private static int _Variable_String_FetchCount;
    private static int _RobotStatus_FetchCount;
    private static int _Alarm_FetchCount;
    private static int _AlarmHist_FetchCount;
    private static int _IO_General_Input_FetchCount;
    private static int _IO_General_Output_FetchCount;
    private static int _IO_External_Input_FetchCount;
    private static int _IO_External_Output_FetchCount;
    private static int _IO_Network_Input_FetchCount;
    private static int _IO_Network_Output_FetchCount;
    private static int _IO_Specific_Input_FetchCount;
    private static int _IO_Specific_Output_FetchCount;
    private static int _IO_AuxiliaryRelay_FetchCount;
    private static int _IO_Internal_Control_Status_FetchCount;
    private static int _IO_Pseudo_Input_FetchCount;
    private static int _GetManagementTime_FetchCount;
    private static int _JobData_FetchCount;
    private static int _TorkData_FetchCount;
    private static int _AbsoData_FetchCount;
    private static int _TorkExamJobList_FetchCount;
    private static int _TorkExamJobSelect_FetchCount;
    private static int _TorkExamTork_FetchCount;
    private static int _TorkExamJob_FetchCount;
    private static int _TorkExamSignal_FetchCount;

    private static int _Register_FetchCount;



    private static int _Variable_Byte_SendCount;
    private static int _Variable_Integer_SendCount;
    private static int _Variable_Double_SendCount;
    private static int _Variable_Real_SendCount;
    private static int _Variable_String_SendCount;
    private static int _RobotStatus_SendCount;
    private static int _Alarm_SendCount;
    private static int _AlarmHist_SendCount;
    private static int _IO_General_Input_SendCount;
    private static int _IO_General_Output_SendCount;
    private static int _IO_External_Input_SendCount;
    private static int _IO_External_Output_SendCount;
    private static int _IO_Network_Input_SendCount;
    private static int _IO_Network_Output_SendCount;
    private static int _IO_Specific_Input_SendCount;
    private static int _IO_Specific_Output_SendCount;
    private static int _IO_AuxiliaryRelay_SendCount;
    private static int _IO_Internal_Control_Status_SendCount;
    private static int _IO_Pseudo_Input_SendCount;
    private static int _GetManagementTime_SendCount;
    private static int _JobData_SendCount;
    private static int _TorkData_SendCount;
    private static int _AbsoData_SendCount;
    private static int _TorkExamJobList_SendCount;
    private static int _TorkExamJobSelect_SendCount;
    private static int _TorkExamTork_SendCount;
    private static int _TorkExamJob_SendCount;
    private static int _TorkExamSignal_SendCount;

    private static int _Register_SendCount;


    #endregion

    #region HELP
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> _robotTasks = new ConcurrentDictionary<string, CancellationTokenSource>();

    private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        MaxDepth = 10,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private static readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;
    private const int DegiskenSayisi = 100;
    private static WebSocketApiService _staticApiService; // Program class'ının static member'ı
    #endregion

    // Manual backup semaphore - Max 2 eş zamanlı manual backup
    private static readonly SemaphoreSlim _manualBackupSemaphore = new SemaphoreSlim(2, 2);

    public class RobotService : BackgroundService
    {
        private readonly WebSocketApiService _apiService;

        public RobotService(WebSocketApiService apiService)
        {
            _apiService = apiService;
            _staticApiService = apiService; // Set static instance for use in static methods
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Test WebSocket API connection instead of database
                try
                {
                    var testRobots = await _apiService.GetActiveRobotsAsync();
                    Console.WriteLine($"{DateTime.Now} WebSocket API bağlantısı başarılı. {testRobots.Count} aktif robot bulundu.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} WebSocket API bağlantı hatası: {ex.Message}");
                    return;
                }

                var tasks = new List<Task>
                {
                    Program.ProcessBackupSchedules(stoppingToken),
                    Program.MonitorRobotIPs(stoppingToken),
                    //Program.MonitorSystem(stoppingToken)
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} Servis çalışırken hata oluştu: {ex.Message}");
                throw;
            }
        }
    }
    static async Task Main(string[] args)
    {
        try
        {
            // Watchlog ana dizini oluşturma
            string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
            if (!Directory.Exists(watchlogDirectory))
            {
                Directory.CreateDirectory(watchlogDirectory);
                Console.WriteLine($"{DateTime.Now} Watchlog klasörü oluşturuldu: {watchlogDirectory}");
            }

            // Log dizini oluşturma
            string logDirectory = Path.Combine(watchlogDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
                Console.WriteLine($"{DateTime.Now} Log klasörü oluşturuldu: {logDirectory}");
            }

            // Log dosyası kontrolü ve oluşturma için Timer başlat
            var logTimer = new Timer(async _ =>
            {
                try
                {
                    string currentLogFileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
                    string currentLogPath = Path.Combine(logDirectory, currentLogFileName);

                    if (!File.Exists(currentLogPath))
                    {
                        var logStream = new StreamWriter(currentLogPath, true) { AutoFlush = true };
                        var multiWriter = new MultiTextWriter(new TextWriter[] { Console.Out, logStream });
                        Console.SetOut(multiWriter);
                        Console.WriteLine($"{DateTime.Now} Yeni log dosyası oluşturuldu: {currentLogPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} Log dosyası kontrolü sırasında hata: {ex.Message}");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); // Her dakika kontrol et

            // İlk log dosyasını oluştur
            string initialLogFileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
            string initialLogPath = Path.Combine(logDirectory, initialLogFileName);
            var initialLogStream = new StreamWriter(initialLogPath, true) { AutoFlush = true };
            var initialMultiWriter = new MultiTextWriter(new TextWriter[] { Console.Out, initialLogStream });
            Console.SetOut(initialMultiWriter);

            Console.WriteLine($"{DateTime.Now} Uygulama başlatıldı");
            Console.WriteLine($"{DateTime.Now} Log dosyası: {initialLogPath}");

            var builder = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "RobotBackupService";
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<WebSocketApiService>();
                    services.AddHostedService<RobotService>();
                });

            await builder.Build().RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Uygulama başlatılırken hata oluştu: {ex.Message}");
            throw;
        }
    }

    // Console çıktılarını birden fazla yazıcıya yönlendirmek için yardımcı sınıf
    public class MultiTextWriter : TextWriter
    {
        private readonly IEnumerable<TextWriter> _writers;

        public MultiTextWriter(IEnumerable<TextWriter> writers)
        {
            _writers = writers;
        }

        public override void Write(char value)
        {
            foreach (var writer in _writers)
                writer.Write(value);
        }

        public override void Write(string value)
        {
            foreach (var writer in _writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in _writers)
                writer.Flush();
        }

        public override void Close()
        {
            foreach (var writer in _writers)
                writer.Close();
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
    private static async Task ProcessBackupSchedules(CancellationToken ct)
    {
        List<Task> BackupTasks = new List<Task>();

        DateTime lastCheck = DateTime.MinValue;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now;

                // Her dakika başında kontrol et
                if (lastCheck.Minute != now.Minute)
                {
                    if (_staticApiService == null)
                    {
                        Console.WriteLine($"{DateTime.Now} WebSocket API service is not initialized");
                        continue;
                    }

                    var schedules = await _staticApiService.GetBackupSchedulesAsync();

                    // Filter schedules based on time and day
                    var filteredSchedules = schedules
                        .Where(s => s.is_active)
                        .Where(s => s.days.Contains((int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek))
                        .Where(s => s.time.Hours == now.Hour)
                        .Where(s => s.time.Minutes == now.Minute)
                        .ToList();

                    foreach (var schedule in filteredSchedules)
                    {
                        var activeRobots = await _staticApiService.GetActiveRobotsAsync();

                        foreach (var robot in activeRobots)
                        {
                            BackupTasks.Add(Task.Run(() => BackupRobotData(robot, schedule)));
                        }
                        await Task.WhenAll(BackupTasks);
                    }
                    lastCheck = now;
                }
                await Task.Delay(10000, ct); // 10 saniyede bir kontrol et
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} Yedekleme zamanlaması hatası: {ex.Message}");
                await Task.Delay(10000, ct);
            }
        }
    }
    private static async Task BackupRobotData(RobotIP robot, BackupSchedules schedule)
    {
        string BackupDirectory = @"C:\";
        string backupBasePath = Path.Combine(BackupDirectory, "Backup");
        if (!Directory.Exists(backupBasePath))
        {
            Directory.CreateDirectory(backupBasePath);
            Console.WriteLine($"{DateTime.Now} Yedek klasörü oluşturuldu: {backupBasePath}");
        }

        string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
        // Log dizini oluşturma
        string logDirectory = Path.Combine(watchlogDirectory, "BackupLogs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
            Console.WriteLine($"{DateTime.Now} BackupLogs klasörü oluşturuldu: {logDirectory}");
        }

        var now = DateTime.Now;

        string LogKaydi = logDirectory + $"\\{robot.name}_{robot.ip_address}_{now:yyyy-MM-dd}_{schedule.time.Hours}_{schedule.time.Minutes}_Backup_Log.txt";

        List<List<string>> fileLists = new List<List<string>>();
        List<string> ErrorFileBackups = new List<string>();

        bool BackupStatus = true;

        bool CmosTarget = false;

        try
        {
            if (robot.id == schedule.controller_id)
            {
                //MotomanController c = MotomanController.OpenConnection(robot.ip_address, out StatusInfo status);

                var c = RobotConnection.GetClient(robot.ip_address, out bool RobotConnect);

                if (RobotConnect)
                {
                    string folderName = $"{robot.name}_{robot.ip_address}_{now:yyyy-MM-dd}_{schedule.time.Hours}_{schedule.time.Minutes}";
                    string backupPath = Path.Combine(backupBasePath, folderName);

                    if (!Directory.Exists(backupPath))
                    {
                        Directory.CreateDirectory(backupPath);
                        Console.WriteLine($"{DateTime.Now} Yedek klasörü oluşturuldu: {backupPath}");

                        if (BackupStatus)
                        {
                            var statusData = new
                            {
                                type = "robotStatus",
                                data = new
                                {
                                    ip_address = robot.ip_address,
                                    values = new
                                    {
                                        c_backup = true,
                                    }
                                }
                            };

                            var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} Robot Status Data: {jsonData}");

                            QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                            Interlocked.Increment(ref _RobotStatus_FetchCount);

                            BackupStatus = false;
                        }

                        try
                        {
                            foreach (var fileType in schedule.file_types)
                            {
                                switch (fileType)
                                {
                                    case ".jbi":
                                        c.FileList("*.JBI", out List<string> fileList1);
                                        fileLists.Add(fileList1);
                                        break;
                                    case ".dat":
                                        c.FileList("*.DAT", out List<string> fileList2);
                                        fileLists.Add(fileList2);
                                        break;
                                    case ".cnd":
                                        c.FileList("*.CND", out List<string> fileList3);
                                        fileLists.Add(fileList3);
                                        break;
                                    case ".prm":
                                        c.FileList("*.PRM", out List<string> fileList4);
                                        fileLists.Add(fileList4);
                                        break;
                                    case ".sys":
                                        c.FileList("*.SYS", out List<string> fileList5);
                                        fileLists.Add(fileList5);
                                        break;
                                    case ".lst":
                                        c.FileList("*.LST", out List<string> fileList6);
                                        fileLists.Add(fileList6);
                                        break;
                                    case ".log":
                                        c.FileList("*.LOG", out List<string> fileList7);
                                        fileLists.Add(fileList7);
                                        break;
                                    case "CMOS":
                                        CmosTarget = true;
                                        break;
                                }
                            }

                            if (CmosTarget)
                            {
                                string ipAddress = robot.ip_address;
                                string username = "ftp";
                                string password = "";
                                string remoteFilePath = "/spdrv/CMOSBK.BIN";
                                string targetPath = $"{backupPath}\\CMOS.BIN";

                                try
                                {
                                    using (WebClient ftpClient = new WebClient())
                                    {
                                        ftpClient.Credentials = new NetworkCredential(username, password);
                                        string ftpPath = $"ftp://{ipAddress}{remoteFilePath}";

                                        try
                                        {
                                            ftpClient.DownloadFile(ftpPath, targetPath);
                                            Console.WriteLine($"{DateTime.Now} {robot.ip_address} CMOS Dosyası başarıyla indirildi.");
                                        }
                                        catch (WebException ex)
                                        {
                                            if (ex.Response is FtpWebResponse response && response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                                            {
                                                Console.WriteLine($"{DateTime.Now} {robot.ip_address} CMOS dosyası bulunamadı.");
                                            }
                                            else
                                            {
                                                Console.WriteLine($"{DateTime.Now} {robot.ip_address} FTP Hatası: {ex.Message}");
                                            }

                                            if (!File.Exists(LogKaydi)) // Dosya yoksa oluştur
                                            {
                                                File.WriteAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - CMOS dosyasını alamadı." + Environment.NewLine);
                                            }
                                            else // Dosya varsa metni ekle
                                            {
                                                File.AppendAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - CMOS dosyasını alamadı." + Environment.NewLine);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"{DateTime.Now} {robot.ip_address} Genel Hata: {ex.Message}");

                                    if (!File.Exists(LogKaydi)) // Dosya yoksa oluştur
                                    {
                                        File.WriteAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - CMOS dosyasını alamadı." + Environment.NewLine);
                                    }
                                    else // Dosya varsa metni ekle
                                    {
                                        File.AppendAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - CMOS dosyasını alamadı." + Environment.NewLine);
                                    }
                                }
                            }


                            for (int a = 0; a < fileLists.Count; a++)
                            {
                                try
                                {
                                    for (int i = 0; i < fileLists[a].Count; i++)
                                    {
                                        bool DURUM = c.FileSave(fileLists[a][i], backupPath); 

                                        if (DURUM)
                                        {
                                            Console.WriteLine($"{DateTime.Now} {robot.ip_address} {fileLists[a][i]} adlı dosya başarıyla indirildi.    {DURUM}");
                                        }
                                        else
                                        {
                                            ErrorFileBackups.Add(fileLists[a][i]);
                                            Console.WriteLine($"{DateTime.Now} {robot.ip_address} {fileLists[a][i]} adlı dosya indirmede sorun yaşandı!!!.    {DURUM}");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"{DateTime.Now} {robot.ip_address} {fileLists[a]} dosyası yedeklenirken hata: {ex.Message}");
                                }
                            }

                            if (ErrorFileBackups.Count == 0)
                            {
                                Console.WriteLine($"{DateTime.Now} {robot.ip_address} {backupPath} backup başarıyla alındı");
                            }
                            else
                            {
                                Console.WriteLine($"{DateTime.Now} {robot.ip_address} {backupPath} aşağıda belirtilen dosyaları alamadı");

                                foreach (string file in ErrorFileBackups)
                                {
                                    Console.WriteLine($"{DateTime.Now} {robot.ip_address} - {file} dosyasını alamadı.");
                                    Console.WriteLine($"{DateTime.Now} {robot.ip_address} - {file} dosyasını alamayı tekrar deniyor...");

                                    bool DURUM = c.FileSave(file, backupPath);

                                    if (DURUM)
                                    {
                                        Console.WriteLine($"{DateTime.Now} {robot.ip_address} {file} adlı dosya başarıyla indirildi.    {DURUM}");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"{DateTime.Now} {robot.ip_address} {file} adlı dosya indirmede sorun yaşandı!!!.    {DURUM}");

                                        if (!File.Exists(LogKaydi)) // Dosya yoksa oluştur
                                        {
                                            File.WriteAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - {file} dosyasını alamadı." + Environment.NewLine);
                                        }
                                        else // Dosya varsa metni ekle
                                        {
                                            File.AppendAllText(LogKaydi, $"{DateTime.Now} {robot.ip_address} - {file} dosyasını alamadı." + Environment.NewLine);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now} Dosya yedekleme hatası ({robot.ip_address}): {ex.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} Dosya yedekleme hatası: {robot.ip_address} ip adresli bağlantı olmadığından dolayı backup alınamadı");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Yedekleme klasörü oluşturma hatası ({robot.ip_address}): {ex.Message}");
        }
        finally
        {
            if (robot.id == schedule.controller_id)
            {
                var statusData = new
                {
                    type = "robotStatus",
                    data = new
                    {
                        ip_address = robot.ip_address,
                        values = new
                        {
                            c_backup = false,
                        }
                    }
                };

                var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                Console.WriteLine($"{DateTime.Now} Robot Status Datatt: {jsonData}");

                QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                Interlocked.Increment(ref _RobotStatus_FetchCount);
            }
        }

    }
    private static async Task MonitorRobotIPs(CancellationToken ct)
    {
        const int reconnectDelay = 5000; // 5 saniye
        bool wasConnected = false;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var client = new ClientWebSocket();
                Console.WriteLine($"{DateTime.Now} WebSocket bağlantısı deneniyor...");

                try
                {
                    await client.ConnectAsync(new Uri("ws://10.0.110.3:4000"), ct);
                    Console.WriteLine($"{DateTime.Now} WebSocket bağlantısı başarılı.");
                    wasConnected = true;

                    // Bağlantı başarılı olduğunda tüm görevleri başlat
                    var wsTask = SendDataToWebSocket(client, ct);

                    // Sunucudan gelen mesajları dinle
                    var buffer = new byte[1024];
                    while (!ct.IsCancellationRequested && client.State == WebSocketState.Open)
                    {
                        var activeRobots = await _staticApiService.GetActiveRobotsAsync();
                        var activeRobotIPs = activeRobots.Select(r => r.ip_address).ToList();

                        bool isFirstMatch = true; // İlk eşleşme olup olmadığını kontrol eden bayrak

                        // Deaktif olan robotların task'lerini durdur
                        foreach (var taskKey in _robotTasks.Keys)
                        {
                            string ipAddress = taskKey.Split('_')[0];

                            if (!activeRobotIPs.Contains(ipAddress))
                            {
                                List<RobotStatusValue> RobotStatusDegeri = await _staticApiService.GetStatusByIpAsync(ipAddress);

                                foreach (var item in RobotStatusDegeri)
                                {
                                    if (item.connection && item.ip_address == ipAddress && isFirstMatch)
                                    {
                                        var statusData = new
                                        {
                                            type = "robotStatus",
                                            data = new
                                            {
                                                ip_address = item.ip_address,
                                                values = new
                                                {
                                                    connection = false,
                                                }
                                            }
                                        };

                                        var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                                        Console.WriteLine($"{DateTime.Now} Robot Status CONNECTION : {jsonData}");

                                        QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                                        Interlocked.Increment(ref _RobotStatus_FetchCount);

                                        isFirstMatch = false;
                                    }
                                }

                                if (_robotTasks.TryRemove(taskKey, out var cts))
                                {
                                    cts.Cancel();
                                    cts.Dispose();
                                    Console.WriteLine($"{DateTime.Now} Robot IP {taskKey} görevi durduruldu.");
                                }
                            }
                        }

                        try
                        {
                            var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                Console.WriteLine($"{DateTime.Now} Sunucu bağlantıyı kapattı.");
                                break;
                            }

                            // Gelen mesajı işle
                            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                            try
                            {
                                var message = JsonConvert.DeserializeObject<WebSocketMessage>(receivedMessage);
                                if (message != null && message.type != "pong")
                                {
                                    Console.WriteLine($"{DateTime.Now} WebSocket mesajı alındı = {JsonConvert.SerializeObject(message, Formatting.None)}");


                                    List<string> RemoveTaskName = new List<string>();

                                    switch (message.type)
                                    {
                                        case "jobExit":
                                            RemoveTaskName.Add("JobData");
                                            RemoveTaskName.Add("JobSelectData");
                                            break;
                                        case "monitoringExit":
                                            RemoveTaskName.Add("TorkData");
                                            break;
                                        case "inputOutputExit":
                                            RemoveTaskName.Add("IO_General_Input");
                                            RemoveTaskName.Add("IO_General_Output");
                                            RemoveTaskName.Add("IO_External_Input");
                                            RemoveTaskName.Add("IO_External_Output");
                                            RemoveTaskName.Add("IO_Network_Input");
                                            RemoveTaskName.Add("IO_Network_Output");
                                            RemoveTaskName.Add("IO_Specific_Input");
                                            RemoveTaskName.Add("IO_Specific_Output");
                                            RemoveTaskName.Add("IO_AuxiliaryRelay");
                                            RemoveTaskName.Add("IO_Internal_Control_Status");
                                            RemoveTaskName.Add("IO_Pseudo_Input");
                                            break;
                                        case "variableExit":
                                            RemoveTaskName.Add("Variable_Byte");
                                            RemoveTaskName.Add("Variable_Integer");
                                            RemoveTaskName.Add("Variable_Double");
                                            RemoveTaskName.Add("Variable_Real");
                                            RemoveTaskName.Add("Variable_String");
                                            break;
                                        case "dataExit":
                                            RemoveTaskName.Add("AbsoData");
                                            break;
                                        case "registerExit":
                                            RemoveTaskName.Add("RegisterData");
                                            break;
                                        default:
                                            break;
                                    }

                                    for (int i = 0; i < RemoveTaskName.Count; i++)
                                    {
                                        string taskKey = message.data.ipAddress + $"_{RemoveTaskName[i]}";

                                        if (_robotTasks.TryRemove(taskKey, out var cts))
                                        {
                                            cts.Cancel();
                                            cts.Dispose();
                                            Console.WriteLine($"{DateTime.Now} -> {taskKey} durduruldu.");
                                        }
                                    }



                                    /*
                                  var existingTasks = _robotTasks.Keys
                                                        .Where(k => k.StartsWith(message.data.ipAddress + "_") &&
                                                        !k.EndsWith("_Alarm") &&
                                                        !k.EndsWith("_RobotStatus") &&
                                                        !k.EndsWith("_AlarmHist") &&
                                                        !k.EndsWith("_GetManagementTime"))
                                                        //!k.EndsWith("_TorkExam"))
                                                        .ToList();

                    

                                    foreach (var existingTaskKey in existingTasks)
                                    {
                                        if (_robotTasks.TryRemove(existingTaskKey, out var existingCts))
                                        {
                                            existingCts.Cancel();
                                            existingCts.Dispose();
                                            //Console.WriteLine($"{DateTime.Now} Önceki görev durduruldu - IP: {message.data.ipAddress}, Task: {existingTaskKey}");
                                        }
                                    }
                                    */

                                    /*
                                    if (message.type == "monitoringExit")
                                    {
                                        string taskKey = message.data.ipAddress + "_TorkData";

                                        if (_robotTasks.TryRemove(taskKey, out var cts))
                                        {
                                            cts.Cancel();
                                            cts.Dispose();
                                            Console.WriteLine($"{DateTime.Now} -> {taskKey} durduruldu.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"{DateTime.Now} -> {taskKey} bulunamadı.");
                                        }
                                    }
                                    */

                                    Task task = null;
                                    switch (message.type)
                                    {
                                        case "job":
                                            task = JobData(message.data.ipAddress);
                                            break;
                                        case "jobSelect":
                                            task = JobSelectData(message.data.ipAddress, message.data.type);
                                            break;
                                        case "tork":
                                            task = TorkData(message.data.ipAddress);
                                            break;
                                        case "absoData":
                                            task = AbsoData(message.data.ipAddress);
                                            //task = RegisterData(message.data.ipAddress);
                                            break;
                                        case "register":
                                            task = RegisterData(message.data.ipAddress);
                                            break;
                                        case "registerExit":
                                            StopMonitoringTask($"{message.data.ipAddress}_RegisterData", "Register");
                                            break;
                                        case "absoDataExit":
                                            StopMonitoringTask($"{message.data.ipAddress}_AbsoData", "AbsoData");
                                            break;
                                        case "manualBackup":
                                            Console.WriteLine($"{DateTime.Now} ManualBackup WebSocket mesajı alındı - IP: {message.data.ipAddress}, RequestId: {message.data.requestId}");
                                            task = ManualBackupData(message.data.ipAddress, message.data.controllerName, message.data.fileTypes, message.data.requestId, message.data.controllerId);
                                            break;
                                        /*
                                    case "torkExam":

                                        WSMes_TorkExam_Type = message.data.type;

                                        if (message.data.type == "Init")
                                        {
                                            task = TorkExam(message.data.ipAddress);
                                        }
                                        else if (message.data.type == "JobSelect")
                                        {
                                            if (message.data.values[0].JobName != null)
                                            {
                                                WSMes_TorkExam_SelectJob = message.data.values[0].JobName;
                                            }
                                        }
                                        else if (message.data.type == "Start")
                                        {
                                            if (message.data.values[0].signalNumbers != null)
                                            {
                                                WSMes_TorkExam_SignalNo = message.data.values[0].signalNumbers;
                                            }

                                            if (message.data.values[0].JobName != null)
                                            {
                                                WSMes_TorkExam_JobName = message.data.values[0].JobName;
                                            }

                                            WSMes_TorkExam_Time = message.data.values[0].duration;
                                        }

                                        break;
                                        */
                                        case "byte":
                                            task = Variable_Byte(message.data.ipAddress);
                                            break;
                                        case "int":
                                            task = Variable_Integer(message.data.ipAddress);
                                            break;
                                        case "double":
                                            task = Variable_Double(message.data.ipAddress);
                                            break;
                                        case "real":
                                            task = Variable_Real(message.data.ipAddress);
                                            break;
                                        case "string":
                                            task = Variable_String(message.data.ipAddress);
                                            break;
                                        case "univInput":
                                            task = IO_General_Input(message.data.ipAddress);
                                            break;
                                        case "univOutput":
                                            task = IO_General_Output(message.data.ipAddress);
                                            break;
                                        case "extInput":
                                            task = IO_External_Input(message.data.ipAddress);
                                            break;
                                        case "extOutput":
                                            task = IO_External_Output(message.data.ipAddress);
                                            break;
                                        case "netInput":
                                            task = IO_Network_Input(message.data.ipAddress);
                                            break;
                                        case "netOutput":
                                            task = IO_Network_Output(message.data.ipAddress);
                                            break;
                                        case "spesInput":
                                            task = IO_Specific_Input(message.data.ipAddress);
                                            break;
                                        case "spesOutput":
                                            task = IO_Specific_Output(message.data.ipAddress);
                                            break;
                                        case "auxRel":
                                            task = IO_AuxiliaryRelay(message.data.ipAddress);
                                            break;
                                        case "contStat":
                                            task = IO_Internal_Control_Status(message.data.ipAddress);
                                            break;
                                        case "pseInput":
                                            task = IO_Pseudo_Input(message.data.ipAddress);
                                            break;

                                    }

                                    if (task != null)
                                    {
                                        //Console.WriteLine($"{DateTime.Now} Yeni görev başlatıldı - IP: {message.data.ipAddress}, Tip: {message.type}");
                                        _ = task.ContinueWith(t =>
                                        {
                                            if (t.IsFaulted)
                                            {
                                                Console.WriteLine($"{DateTime.Now} Görev hata ile sonlandı - IP: {message.data.ipAddress}, Tip: {message.type}, Hata: {t.Exception}");
                                            }
                                        }, TaskContinuationOptions.OnlyOnFaulted);
                                    }
                                }
                            }
                            catch (JsonException ex)
                            {
                                Console.WriteLine($"{DateTime.Now} JSON ayrıştırma hatası: {ex.Message}");
                            }
                        }
                        catch (WebSocketException)
                        {
                            // WebSocket hatası oluştuğunda döngüden çık
                            break;
                        }

                        // Aktif robotlar için task'leri başlat
                        foreach (var ip in activeRobotIPs)
                        {
                            List<RobotStatusValue> RobotStatusDegeri = await _staticApiService.GetStatusByIpAsync(ip);

                            var robot = RobotConnection.GetClient(ip, out bool RobotConnect);
                            if (!RobotConnect)
                            {
                                Console.WriteLine($"{DateTime.Now} Robot {ip} bağlantısı sağlanamadı, görevler başlatılmayacak. ");

                                foreach (var item in RobotStatusDegeri)
                                {
                                    if (item.connection && item.ip_address == ip)
                                    {
                                        var statusData = new
                                        {
                                            type = "robotStatus",
                                            data = new
                                            {
                                                ip_address = item.ip_address,
                                                values = new
                                                {
                                                    connection = false,
                                                }
                                            }
                                        };

                                        var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                                        Console.WriteLine($"{DateTime.Now} Robot Status CONNECTION : {jsonData}");

                                        QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                                        Interlocked.Increment(ref _RobotStatus_FetchCount);
                                    }
                                }

                                foreach (var taskKey in _robotTasks.Keys)
                                {
                                    string ipAddress = taskKey.Split('_')[0];

                                    if (ipAddress == ip)
                                    {
                                        if (_robotTasks.TryRemove(taskKey, out var cts))
                                        {
                                            cts.Cancel();
                                            cts.Dispose();
                                            Console.WriteLine($"{DateTime.Now} Robot IP {taskKey} görevi durduruldu.");
                                        }
                                    }
                                }
                                continue;
                            }


                            foreach (var item in RobotStatusDegeri)
                            {
                                if (!item.connection && item.ip_address == ip)
                                {
                                    var statusData = new
                                    {
                                        type = "robotStatus",
                                        data = new
                                        {
                                            ip_address = item.ip_address,
                                            values = new
                                            {
                                                connection = true,
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                                    Console.WriteLine($"{DateTime.Now} Robot Status CONNECTION : {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                                    Interlocked.Increment(ref _RobotStatus_FetchCount);
                                }
                            }

                            if (!_robotTasks.ContainsKey(ip))
                            {
                                var taskTypes = new[] { "RobotStatus", "Alarm", "AlarmHist", "GetManagementTime" };

                                foreach (var taskType in taskTypes)
                                {
                                    string taskKey = $"{ip}_{taskType}";
                                    if (!_robotTasks.ContainsKey(taskKey))
                                    {
                                        var cts = new CancellationTokenSource();
                                        if (_robotTasks.TryAdd(taskKey, cts))
                                        {
                                            Task task = null;
                                            switch (taskType)
                                            {
                                                case "RobotStatus":
                                                    task = RobotStatus(ip);
                                                    break;
                                                case "Alarm":
                                                    task = Alarm(ip);
                                                    break;
                                                case "AlarmHist":
                                                    task = AlarmHist(ip);
                                                    break;
                                                case "GetManagementTime":
                                                    task = GetManagementTime(ip);
                                                    break;
                                            }

                                            if (task != null)
                                            {
                                                _ = task.ContinueWith(t =>
                                                {
                                                    if (t.IsFaulted)
                                                    {
                                                        Console.WriteLine($"{DateTime.Now} Robot {ip} için {taskType} görevi hata ile sonlandı: {t.Exception}");
                                                    }
                                                }, TaskContinuationOptions.OnlyOnFaulted);

                                                //Console.WriteLine($"{DateTime.Now} Robot IP {ip} için {taskType} görevi başlatıldı.");
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        await Task.Delay(30, ct);
                    }
                }
                catch (WebSocketException ex)
                {
                    if (wasConnected)
                    {
                        Console.WriteLine($"{DateTime.Now} WebSocket bağlantısı koptu: {ex.Message}");
                        wasConnected = false;
                    }
                    else
                    {
                        Console.WriteLine($"{DateTime.Now} WebSocket bağlantısı başarısız: {ex.Message}");
                    }
                }

                // Bağlantı koptuğunda veya hata oluştuğunda tüm görevleri temizle
                foreach (var cts in _robotTasks.Values)
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                _robotTasks.Clear();

                if (client.State == WebSocketState.Open)
                {
                    try
                    {
                        await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bağlantı kapatılıyor", ct);
                    }
                    catch { }
                }

                // Yeniden bağlanmadan önce bekle
                Console.WriteLine($"{DateTime.Now} {reconnectDelay / 1000} saniye sonra yeniden bağlanmaya çalışılacak...");
                await Task.Delay(reconnectDelay, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} Beklenmeyen hata oluştu: {ex.Message}");
                await Task.Delay(reconnectDelay, ct);
            }
        }
    }
    private static async Task SendDataToWebSocket(ClientWebSocket client, CancellationToken ct)
    {
        // Ping/Pong için zamanlayıcı
        using var pingTimer = new Timer(async _ =>
        {
            try
            {
                if (client.State == WebSocketState.Open)
                {
                    var pingMessage = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                    await client.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, ct);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} Ping gönderimi sırasında hata: {ex.Message}");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(15)); // Her 15 saniyede bir ping gönder


        while (!ct.IsCancellationRequested && client.State == WebSocketState.Open)
        {
            try
            {
                var tasks = new[]
                {
                    ProcessQueue(_Variable_Byte_Queue, client, "Variable_Byte", ct),
                    ProcessQueue(_Variable_Integer_Queue, client, "Variable_Integer", ct),
                    ProcessQueue(_Variable_Double_Queue, client, "Variable_Double", ct),
                    ProcessQueue(_Variable_Real_Queue, client, "Variable_Real", ct),
                    ProcessQueue(_Variable_String_Queue, client, "Variable_String", ct),
                    ProcessQueue(_RobotStatus_Queue, client, "RobotStatus", ct),
                    ProcessQueue(_Alarm_Queue, client, "Alarm", ct),
                    ProcessQueue(_AlarmHist_Queue, client, "AlarmHist", ct),
                    ProcessQueue(_IO_General_Input_Queue, client, "IO_General_Input", ct),
                    ProcessQueue(_IO_General_Output_Queue, client, "IO_General_Output", ct),
                    ProcessQueue(_IO_External_Input_Queue, client, "IO_External_Input", ct),
                    ProcessQueue(_IO_External_Output_Queue, client, "IO_External_Output", ct),
                    ProcessQueue(_IO_Network_Input_Queue, client, "IO_Network_Input", ct),
                    ProcessQueue(_IO_Network_Output_Queue, client, "IO_Network_Output", ct),
                    ProcessQueue(_IO_Specific_Input_Queue, client, "IO_Specific_Input", ct),
                    ProcessQueue(_IO_Specific_Output_Queue, client, "IO_Specific_Output", ct),
                    ProcessQueue(_IO_AuxiliaryRelay_Queue, client, "IO_AuxiliaryRelay", ct),
                    ProcessQueue(_IO_Internal_Control_Status_Queue, client, "IO_Internal_Control_Status", ct),
                    ProcessQueue(_IO_Pseudo_Input_Queue, client, "IO_Pseudo_Input", ct),
                    ProcessQueue(_GetManagementTime_Queue, client, "GetManagementTime", ct),
                    ProcessQueue(_JobData_Queue, client, "JobData", ct),
                    ProcessQueue(_TorkData_Queue, client, "TorkData", ct),
                    ProcessQueue(_AbsoData_Queue, client, "AbsoData", ct),
                    ProcessQueue(_TorkExamJobList_Queue, client, "TorkExam", ct),
                    ProcessQueue(_TorkExamJobSelect_Queue, client, "TorkExam", ct),
                    ProcessQueue(_TorkExamTork_Queue, client, "TorkExam", ct),
                    ProcessQueue(_TorkExamJob_Queue, client, "TorkExam", ct),
                    ProcessQueue(_TorkExamSignal_Queue, client, "TorkExam", ct),

                    ProcessQueue(_Register_Queue, client, "RegisterData", ct),


                };

                await Task.WhenAll(tasks);
                await Task.Delay(TaskDelays.WEBSOCKET_SEND_DELAY, ct);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"{DateTime.Now} WebSocket veri gönderimi sırasında hata: {ex.Message}");
                break;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} Veri gönderimi sırasında beklenmeyen hata: {ex.Message}");
                await Task.Delay(1000, ct);
            }
        }

    }

    #region KUYRUK
    private static void IncrementCounter(string queueType)
    {
        switch (queueType)
        {
            case "Variable_Byte": Interlocked.Increment(ref _Variable_Byte_SendCount); break;
            case "Variable_Integer": Interlocked.Increment(ref _Variable_Integer_SendCount); break;
            case "Variable_Double": Interlocked.Increment(ref _Variable_Double_SendCount); break;
            case "Variable_Real": Interlocked.Increment(ref _Variable_Real_SendCount); break;
            case "Variable_String": Interlocked.Increment(ref _Variable_String_SendCount); break;
            case "RobotStatus": Interlocked.Increment(ref _RobotStatus_SendCount); break;
            case "Alarm": Interlocked.Increment(ref _Alarm_SendCount); break;
            case "AlarmHist": Interlocked.Increment(ref _AlarmHist_SendCount); break;
            case "IO_General_Input": Interlocked.Increment(ref _IO_General_Input_SendCount); break;
            case "IO_General_Output": Interlocked.Increment(ref _IO_General_Output_SendCount); break;
            case "IO_External_Input": Interlocked.Increment(ref _IO_External_Input_SendCount); break;
            case "IO_External_Output": Interlocked.Increment(ref _IO_External_Output_SendCount); break;
            case "IO_Network_Input": Interlocked.Increment(ref _IO_Network_Input_SendCount); break;
            case "IO_Network_Output": Interlocked.Increment(ref _IO_Network_Output_SendCount); break;
            case "IO_Specific_Input": Interlocked.Increment(ref _IO_Specific_Input_SendCount); break;
            case "IO_Specific_Output": Interlocked.Increment(ref _IO_Specific_Output_SendCount); break;
            case "IO_AuxiliaryRelay": Interlocked.Increment(ref _IO_AuxiliaryRelay_SendCount); break;
            case "IO_Internal_Control_Status": Interlocked.Increment(ref _IO_Internal_Control_Status_SendCount); break;
            case "IO_Pseudo_Input": Interlocked.Increment(ref _IO_Pseudo_Input_SendCount); break;
            case "GetManagementTime": Interlocked.Increment(ref _GetManagementTime_SendCount); break;
            case "JobData": Interlocked.Increment(ref _JobData_SendCount); break;
            case "TorkData": Interlocked.Increment(ref _TorkData_SendCount); break;
            case "AbsoData": Interlocked.Increment(ref _AbsoData_SendCount); break;
            case "TorkExamJoblist": Interlocked.Increment(ref _TorkExamJobList_SendCount); break;
            case "TorkExamJobSelect": Interlocked.Increment(ref _TorkExamJobSelect_SendCount); break;
            case "TorkExamTork": Interlocked.Increment(ref _TorkExamTork_SendCount); break;
            case "TorkExamJob": Interlocked.Increment(ref _TorkExamJob_SendCount); break;
            case "TorkExamSignal": Interlocked.Increment(ref _TorkExamSignal_SendCount); break;

            case "RegisterData": Interlocked.Increment(ref _Register_SendCount); break;

        }
    }
    private static async Task ProcessQueue(ConcurrentQueue<string> queue, ClientWebSocket client, string queueType, CancellationToken ct)
    {
        try
        {
            while (queue.TryDequeue(out string data) && !ct.IsCancellationRequested)
            {
                if (client.State != WebSocketState.Open)
                {
                    Console.WriteLine($"{DateTime.Now} WebSocket bağlantısı kapalı, veri gönderilemiyor.");
                    break;
                }

                //Console.WriteLine($"{DateTime.Now} Kuyruktan veri alındı, gönderilmeye çalışılıyor...");
                var buffer = _bytePool.Rent(Encoding.UTF8.GetByteCount(data));
                try
                {
                    var byteCount = Encoding.UTF8.GetBytes(data, 0, data.Length, buffer, 0);
                    await client.SendAsync(new ArraySegment<byte>(buffer, 0, byteCount), WebSocketMessageType.Text, true, ct);
                    IncrementCounter(queueType);
                    //Console.WriteLine($"{DateTime.Now} Veri başarıyla gönderildi ve sayaç artırıldı.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DateTime.Now} Veri gönderimi sırasında hata: {ex.Message}");
                    throw;
                }
                finally
                {
                    _bytePool.Return(buffer);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} ProcessQueue sırasında hata: {ex.Message}");
            throw;
        }
    }

    #endregion

    #region IO

    static int sinyalNumarasi_General_Input = 256;
    static byte[] ValueHistory_General_Input = new byte[sinyalNumarasi_General_Input];

    static int sinyalNumarasi_General_Output = 256;
    static byte[] ValueHistory_General_Output = new byte[sinyalNumarasi_General_Output];

    static int sinyalNumarasi_External_Input = 256;
    static byte[] ValueHistory_External_Input = new byte[sinyalNumarasi_External_Input];

    static int sinyalNumarasi_External_Output = 256;
    static byte[] ValueHistory_External_Output = new byte[sinyalNumarasi_External_Output];

    static byte[] ValueHistory_Network_Input = new byte[256];

    static byte[] ValueHistory_Network_Output = new byte[256];

    static byte[] ValueHistory_Specific_Input = { };  // Boş bir dizi (0 elemanlı)

    static byte[] ValueHistory_Specific_Output = { };  // Boş bir dizi (0 elemanlı)

    static int sinyalNumarasi_AuxiliaryRelay = 256;
    static byte[] ValueHistory_AuxiliaryRelay = new byte[sinyalNumarasi_AuxiliaryRelay];  // Boş bir dizi (0 elemanlı)

    static byte[] ValueHistory_Internal_Control_Status = { };  // Boş bir dizi (0 elemanlı)

    static byte[] ValueHistory_Pseudo_Input = new byte[20];  // Boş bir dizi (0 elemanlı)


    private static async Task IO_General_Input(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_General_Input";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte[] IO;
                        robot.IORead(1, sinyalNumarasi_General_Input, out IO);

                        if (!ValueHistory_General_Input.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_General_Input.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_General_Input[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (i + 1), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_General_Input {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_General_Input_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_General_Input_FetchCount);
                                }
                            }

                            ValueHistory_General_Input = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Genel Giriş Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_General_Input görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_General_Output(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_General_Output";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_General_Output ÇALIŞIYORRRRR");

                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte[] IO;
                        robot.IORead(1001, sinyalNumarasi_General_Output, out IO);

                        if (!ValueHistory_General_Output.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_General_Output.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_General_Output[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (1001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_General_Output {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_General_Output_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_General_Output_FetchCount);
                                }
                            }

                            ValueHistory_General_Output = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Genel Çıkış Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_General_Output görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_External_Input(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_External_Input";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_External_Input ÇALIŞIYORRRRR");

                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte[] IO;
                        robot.IORead(2001, sinyalNumarasi_External_Input, out IO);

                        if (!ValueHistory_External_Input.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_External_Input.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_External_Input[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (2001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_External_Input {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_External_Input_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_External_Input_FetchCount);
                                }
                            }

                            ValueHistory_External_Input = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Harici Giriş Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_External_Input görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_External_Output(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_External_Output";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_External_Output ÇALIŞIYORRRRR");

                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte[] IO;
                        robot.IORead(3001, sinyalNumarasi_External_Output, out IO);

                        if (!ValueHistory_External_Output.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_External_Output.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_External_Output[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (3001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    Console.WriteLine($"{DateTime.Now} IO_External_Output {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_External_Output_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_External_Output_FetchCount);
                                }
                            }

                            ValueHistory_External_Output = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Harici Çıkış Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_External_Output görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Network_Input(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Network_Input";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Network_Input ÇALIŞIYORRRRR");

                try
                {
                    int signalNumber = 0;
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        signalNumber = systemInformation.SystemSoftwareVersion.StartsWith("DS") ? 2501 : 2701;

                        byte[] IO;
                        robot.IORead(signalNumber, 256, out IO);

                        if (!ValueHistory_Network_Input.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_Network_Input.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_Network_Input[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (signalNumber + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_Network_Input {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Network_Input_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_Network_Input_FetchCount);
                                }
                            }

                            ValueHistory_Network_Input = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Ağ Giriş Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Network_Input görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Network_Output(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Network_Output";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Network_Output ÇALIŞIYORRRRR");

                try
                {
                    int signalNumber = 0;
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        signalNumber = systemInformation.SystemSoftwareVersion.StartsWith("DS") ? 3501 : 3701;

                        byte[] IO;
                        robot.IORead(signalNumber, 256, out IO);

                        if (!ValueHistory_Network_Output.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_Network_Output.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_Network_Output[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (signalNumber + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_Network_Output {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Network_Output_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_Network_Output_FetchCount);
                                }
                            }

                            ValueHistory_Network_Output = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Ağ Çıkış Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Network_Output görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Specific_Input(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Specific_Input";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Specific_Input ÇALIŞIYORRRRR");

                try
                {
                    int count = 0;

                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        if (systemInformation.SystemSoftwareVersion.StartsWith("DS"))
                        {
                            count = 160;
                        }
                        else
                        {
                            count = 256;
                        }
                        Array.Resize(ref ValueHistory_Specific_Input, count);

                        byte[] IO;
                        robot.IORead(4001, count, out IO);

                        if (!ValueHistory_Specific_Input.SequenceEqual(IO))
                        {
                            for (int i = 0; i < IO.Length; i++)
                            {
                                if (ValueHistory_Specific_Input[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (4001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} IO_Specific_Input {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Specific_Input_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_Specific_Input_FetchCount);
                                }
                            }

                            ValueHistory_Specific_Input = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Özel Giriş Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Specific_Input görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Specific_Output(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Specific_Output";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Specific_Output ÇALIŞIYORRRRR");

                try
                {
                    int count = 0;
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        if (systemInformation.SystemSoftwareVersion.StartsWith("DS"))
                        {
                            count = 200;
                        }
                        else
                        {
                            count = 256;
                        }

                        Array.Resize(ref ValueHistory_Specific_Output, count);

                        byte[] IO;
                        robot.IORead(5001, count, out IO);

                        if (!ValueHistory_Specific_Output.SequenceEqual(IO))
                        {
                            for (int i = 0; i < IO.Length; i++)
                            {
                                if (ValueHistory_Specific_Output[i] != IO[i] && i != 90)
                                {
                                    var responseData = CreateIOResponse(robotIP, (5001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);

                                    //Console.WriteLine($"{DateTime.Now} IO_Specific_Output {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Specific_Output_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_Specific_Output_FetchCount);
                                }
                                else if (i == 90)
                                {
                                    bool[] HistoryBit = new bool[8];
                                    bool[] IObit = new bool[8];

                                    for (int a = 0; a < 8; a++)
                                    {
                                        IObit[a] = (IO[i] & (1 << a)) != 0;
                                    }
                                    for (int a = 0; a < 8; a++)
                                    {
                                        HistoryBit[a] = (ValueHistory_Specific_Output[i] & (1 << a)) != 0;
                                    }

                                    for (int a = 0; a < 8; a++)
                                    {
                                        if (a == 1) continue; // 1. indexi atla

                                        if (HistoryBit[a] != IObit[a])
                                        {
                                            var responseData = new
                                            {
                                                type = "io",
                                                data = new
                                                {
                                                    ip_address = robotIP,
                                                    values = new[]
                                                    {
                                                    new
                                                    {
                                                        byteNumber = 5001 + i,
                                                        bits = IObit,
                                                    }
                                                }
                                                }
                                            };

                                            var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);

                                            QueueConfig.EnqueueWithLimit(_IO_Specific_Output_Queue, jsonData);
                                            Interlocked.Increment(ref _IO_Specific_Output_FetchCount);
                                        }
                                    }

                                    HistoryBit = IObit; // Önceki değeri güncelle
                                }
                            }

                            ValueHistory_Specific_Output = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Özel Çıkış Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Specific_Output görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_AuxiliaryRelay(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_AuxiliaryRelay";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_AuxiliaryRelay ÇALIŞIYORRRRR");

                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte[] IO;
                        robot.IORead(7001, sinyalNumarasi_AuxiliaryRelay, out IO);

                        if (!ValueHistory_AuxiliaryRelay.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_AuxiliaryRelay.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_AuxiliaryRelay[i] != IO[i] && i != 0)
                                {
                                    var responseData = CreateIOResponse(robotIP, (7001 + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"IO Auxiliary Relay Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_AuxiliaryRelay_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_AuxiliaryRelay_FetchCount);
                                }
                                else if (i == 0)
                                {
                                    bool[] HistoryBit = new bool[8];
                                    bool[] IObit = new bool[8];

                                    for (int a = 0; a < 8; a++)
                                    {
                                        IObit[a] = (IO[i] & (1 << a)) != 0;
                                    }
                                    for (int a = 0; a < 8; a++)
                                    {
                                        HistoryBit[a] = (ValueHistory_AuxiliaryRelay[i] & (1 << a)) != 0;
                                    }

                                    for (int a = 0; a < 8; a++)
                                    {
                                        if (a == 6) continue; // 1. indexi atla

                                        if (HistoryBit[a] != IObit[a])
                                        {
                                            var responseData = new
                                            {
                                                type = "io",
                                                data = new
                                                {
                                                    ip_address = robotIP,
                                                    values = new[]
                                                    {
                                                    new
                                                    {
                                                        byteNumber = 7001 + i,
                                                        bits = IObit,
                                                    }
                                                }
                                                }
                                            };

                                            var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                            Console.WriteLine($"IO Auxiliary Relay Data: {jsonData}");

                                            QueueConfig.EnqueueWithLimit(_IO_AuxiliaryRelay_Queue, jsonData);
                                            Interlocked.Increment(ref _IO_AuxiliaryRelay_FetchCount);
                                        }

                                        HistoryBit = IObit; // Önceki değeri güncelle
                                    }
                                }
                            }
                            ValueHistory_AuxiliaryRelay = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Yardımcı Röle Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_AuxiliaryRelay görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Internal_Control_Status(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Internal_Control_Status";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Internal_Control_Status ÇALIŞIYORRRRR");

                try
                {
                    int count = 0;
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        if (systemInformation.SystemSoftwareVersion.StartsWith("DS"))
                        {
                            count = 64;
                        }
                        else
                        {
                            count = 256;
                        }

                        byte[] IO;
                        robot.IORead(8001, count, out IO);

                        Array.Resize(ref ValueHistory_Internal_Control_Status, count);

                        if (!ValueHistory_Internal_Control_Status.SequenceEqual(IO))
                        {
                            for (int i = 0; i < IO.Length; i++)
                            {
                                if (ValueHistory_Internal_Control_Status[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, 8001 + i, IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"IO Internal Control Status Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Internal_Control_Status_Queue, jsonData);
                                    Interlocked.Increment(ref _IO_Internal_Control_Status_FetchCount);
                                }
                            }

                            ValueHistory_Internal_Control_Status = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO İç Kontrol Durumu Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Internal_Control_Status görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task IO_Pseudo_Input(string robotIP)
    {
        string taskKey = $"{robotIP}_IO_Pseudo_Input";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine($"{DateTime.Now} IO_Pseudo_Input ÇALIŞIYORRRRR");

                try
                {
                    int signalNumber = 0;
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.SystemInformation systemInformation;
                        robot.GetSystemInformation(11, out systemInformation);

                        if (systemInformation.SystemSoftwareVersion.StartsWith("DS"))
                        {
                            signalNumber = 8201;
                        }
                        else
                        {
                            signalNumber = 8701;
                        }

                        byte[] IO;
                        robot.IORead(signalNumber, 20, out IO);

                        if (!ValueHistory_Pseudo_Input.SequenceEqual(IO))
                        {
                            int minLength = Math.Min(ValueHistory_Pseudo_Input.Length, IO.Length);

                            for (int i = 0; i < minLength; i++)
                            {
                                if (ValueHistory_Pseudo_Input[i] != IO[i])
                                {
                                    var responseData = CreateIOResponse(robotIP, (signalNumber + i), IO[i]);
                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"IO Pseudo Input Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_IO_Pseudo_Input_Queue,
                                        jsonData);
                                    Interlocked.Increment(ref _IO_Pseudo_Input_FetchCount);
                                }
                            }

                            ValueHistory_Pseudo_Input = IO;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.IO_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} IO Sözde Giriş Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} IO_Pseudo_Input görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }

    #endregion

    #region ALARM
    private static async Task Alarm(string robotIP)
    {
        bool[] AlarmIsActive = new bool[4];
        Alarm_Data[] alarmArray = new Alarm_Data[4];

        bool[] HistoryAlarmIsActive = new bool[4];
        Alarm_Data[] HistoryAlarmArray = new Alarm_Data[4];

        alarmArray[0] = new Alarm_Data();
        alarmArray[1] = new Alarm_Data();
        alarmArray[2] = new Alarm_Data();
        alarmArray[3] = new Alarm_Data();

        HistoryAlarmArray[0] = new Alarm_Data();
        HistoryAlarmArray[1] = new Alarm_Data();
        HistoryAlarmArray[2] = new Alarm_Data();
        HistoryAlarmArray[3] = new Alarm_Data();

        int i = 1;

        string taskKey = $"{robotIP}_Alarm";
        CancellationTokenSource cts = null;

        bool durum = false;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.Alarm_Data alarmData;
                        robot.ReadAlarmData(i, out alarmData);

                        if (alarmData.AlarmCode != 0)
                        {
                            AlarmIsActive[i - 1] = true;
                            alarmArray[i - 1] = alarmData;

                            if (HistoryAlarmIsActive[i - 1] != AlarmIsActive[i - 1] || !HistoryAlarmArray[i - 1].Equals(alarmArray[i - 1]))
                            {
                                var responseData = new
                                {
                                    type = "alarm",
                                    data = new
                                    {
                                        type = "alarm",
                                        ip_address = robotIP,
                                        values = new[]
                                        {
                                            new
                                            {
                                                code = alarmArray[i - 1].AlarmCode.ToString(),
                                                alarm = alarmArray[i - 1].AlarmType.ToString(),
                                                text = alarmArray[i - 1].AlarmName,
                                                origin_date = alarmArray[i - 1].AlarmTime,
                                                is_active = AlarmIsActive[i - 1]
                                            }
                                        }
                                    }
                                };

                                var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                Console.WriteLine($"{DateTime.Now} Alarm Data: {jsonData}");

                                QueueConfig.EnqueueWithLimit(_Alarm_Queue, jsonData);
                                Interlocked.Increment(ref _Alarm_FetchCount);

                                HistoryAlarmIsActive[i - 1] = true;
                                HistoryAlarmArray[i - 1] = alarmArray[i - 1];
                            }

                            i = (i == 4) ? 1 : i + 1;
                            durum = false;

                            if (!token.IsCancellationRequested)
                            {
                                await Task.Delay(TaskDelays.ALARM_DELAY, token);
                            }
                        }
                        else
                        {
                            AlarmIsActive[i - 1] = false;
                            if (HistoryAlarmIsActive[i - 1] != AlarmIsActive[i - 1] || !HistoryAlarmArray[i - 1].Equals(alarmArray[i - 1]))
                            {
                                var responseData = new
                                {
                                    type = "alarm",
                                    data = new
                                    {
                                        type = "alarm",
                                        ip_address = robotIP,
                                        values = new[]
                                        {
                                             new
                                             {
                                                 code = alarmArray[i - 1].AlarmCode.ToString(),
                                                 alarm = alarmArray[i - 1].AlarmType.ToString(),
                                                 text = alarmArray[i - 1].AlarmName,
                                                 origin_date = alarmArray[i - 1].AlarmTime,
                                                 is_active = false
                                             }
                                         }
                                    }
                                };

                                var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                Console.WriteLine($"{DateTime.Now} Alarm Data: {jsonData}");

                                QueueConfig.EnqueueWithLimit(_Alarm_Queue, jsonData);
                                Interlocked.Increment(ref _Alarm_FetchCount);

                                HistoryAlarmIsActive[i - 1] = false;
                                HistoryAlarmArray[i - 1] = alarmArray[i - 1];

                            }

                            i = 1;
                            durum = true;

                            if (!token.IsCancellationRequested)
                            {
                                await Task.Delay(50, token);
                            }
                        }


                        if (durum)
                        {
                            // AlarmDB nesnesini oluştur
                            AlarmDB alarmDB = new AlarmDB();

                            List<Alarm_Value> alarm = alarmDB.GetByIp(robotIP);

                            List<Alarm_Value> AlarmCodes = new List<Alarm_Value>();

                            byte sayac = 0;

                            if (alarm != null)
                            {
                                for (int j = 0; j < alarm.Count; j++)
                                {
                                    if (alarm[j].ip_address == robotIP)
                                    {
                                        for (int k = 0; k < 4; k++)
                                        {
                                            if ((AlarmIsActive[k] && alarm[j].code == alarmArray[k].AlarmCode.ToString()))
                                            {
                                                AlarmCodes.Add(new Alarm_Value
                                                {
                                                    code = alarm[j].code,
                                                    alarm = alarm[j].alarm,
                                                    text = alarm[j].text,
                                                    origin_date = alarm[j].origin_date,
                                                    is_active = alarm[j].is_active,
                                                    ip_address = alarm[j].ip_address,
                                                });

                                                sayac++;
                                            }
                                        }
                                    }
                                }



                                List<Alarm_Value> farklar = alarm
                                                .Except(AlarmCodes)
                                                .Concat(AlarmCodes.Except(alarm))
                                                .ToList();

                                for (int j = 0; j < farklar.Count; j++)
                                {
                                    var responseData = new
                                    {
                                        type = "alarm",
                                        data = new
                                        {
                                            type = "alarm",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                            new
                                            {
                                                code = farklar[j].code,
                                                alarm = farklar[j].alarm,
                                                text = farklar[j].text,
                                                origin_date = farklar[j].origin_date,
                                                is_active = false
                                            }
                                        }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    Console.WriteLine($"{DateTime.Now} Alarm Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Alarm_Queue, jsonData);
                                    Interlocked.Increment(ref _Alarm_FetchCount);
                                }
                            }

                            durum = false;
                        }

                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Alarm Hatası ({robotIP}): {ex.Message}");

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }

                    i = 1; // Hata durumunda i'yi sıfırla
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Alarm görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }

            // AlarmDB nesnesini oluştur
            AlarmDB alarmDB = new AlarmDB();

            List<Alarm_Value> alarm = alarmDB.GetByIp(robotIP);

            if (alarm != null)
            {
                for (int j = 0; j < alarm.Count; j++)
                {
                    if (alarm[j].ip_address == robotIP)
                    {
                        var responseData = new
                        {
                            type = "alarm",
                            data = new
                            {
                                type = "alarm",
                                ip_address = robotIP,
                                values = new[]
                                {
                                new
                                {
                                    code = alarm[j].code,
                                    alarm = alarm[j].alarm,
                                    text = alarm[j].text,
                                    origin_date = alarm[j].origin_date,
                                    is_active = false
                                }
                            }
                            }
                        };

                        var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                        Console.WriteLine($"{DateTime.Now} Alarm Data: {jsonData}");

                        QueueConfig.EnqueueWithLimit(_Alarm_Queue, jsonData);
                        Interlocked.Increment(ref _Alarm_FetchCount);
                    }
                }

            }

        }
    }
    private static async Task AlarmHist(string robotIP)
    {
        #region ALARM
        // ALARMHIST

        List<string> MAJOR_AlarmCode;
        List<string> MAJOR_AlarmName;
        List<string> MAJOR_AlarmTime;
        List<string> MAJOR_AlarmMode;

        List<string> MINOR_AlarmCode;
        List<string> MINOR_AlarmName;
        List<string> MINOR_AlarmTime;
        List<string> MINOR_AlarmMode;

        List<string> IO_SYS_AlarmCode;
        List<string> IO_SYS_AlarmName;
        List<string> IO_SYS_AlarmTime;
        List<string> IO_SYS_AlarmMode;

        List<string> IO_USR_AlarmCode;
        List<string> IO_USR_AlarmName;
        List<string> IO_USR_AlarmTime;
        List<string> IO_USR_AlarmMode;

        List<string> OFFLINE_AlarmCode;
        List<string> OFFLINE_AlarmName;
        List<string> OFFLINE_AlarmTime;
        List<string> OFFLINE_AlarmMode;

        List<List<string>> AlarmModeList = new List<List<string>>();

        string filePath;
        #endregion

        string JsonHistory = "";
        string jsonData = "a";


        string taskKey = $"{robotIP}_AlarmHist";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                //Console.WriteLine("aaaaaaa");  
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {

                        string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
                        string almhistBasePath = Path.Combine(watchlogDirectory, "Almhist");
                        string robotIPPath = Path.Combine(almhistBasePath, robotIP);

                        // Ana Almhist klasörünü oluştur
                        if (!Directory.Exists(almhistBasePath))
                        {
                            Directory.CreateDirectory(almhistBasePath);
                            Console.WriteLine($"{DateTime.Now} Almhist klasörü oluşturuldu: {almhistBasePath}");
                        }

                        // Robot IP'sine özel klasörü oluştur
                        if (!Directory.Exists(robotIPPath))
                        {
                            Directory.CreateDirectory(robotIPPath);
                            Console.WriteLine($"{DateTime.Now} Robot IP klasörü oluşturuldu: {robotIPPath}");
                        }

                        robot.FileSave("ALMHIST.DAT", robotIPPath);

                        filePath = Path.Combine(robotIPPath, "ALMHIST.DAT");

                        ReadAlarmHist(filePath, "///MAJOR", out MAJOR_AlarmCode, out MAJOR_AlarmName, out MAJOR_AlarmTime, out MAJOR_AlarmMode);
                        ReadAlarmHist(filePath, "///MINOR", out MINOR_AlarmCode, out MINOR_AlarmName, out MINOR_AlarmTime, out MINOR_AlarmMode);
                        ReadAlarmHist(filePath, "///IO_SYS", out IO_SYS_AlarmCode, out IO_SYS_AlarmName, out IO_SYS_AlarmTime, out IO_SYS_AlarmMode);
                        ReadAlarmHist(filePath, "///IO_USR", out IO_USR_AlarmCode, out IO_USR_AlarmName, out IO_USR_AlarmTime, out IO_USR_AlarmMode);
                        ReadAlarmHist(filePath, "///OFFLINE", out OFFLINE_AlarmCode, out OFFLINE_AlarmName, out OFFLINE_AlarmTime, out OFFLINE_AlarmMode);

                        AlarmModeList.Add(MAJOR_AlarmMode);
                        AlarmModeList.Add(MINOR_AlarmMode);
                        AlarmModeList.Add(IO_SYS_AlarmMode);
                        AlarmModeList.Add(IO_USR_AlarmMode);
                        AlarmModeList.Add(OFFLINE_AlarmMode);

                        for (int a = 0; a < AlarmModeList.Count; a++)
                        {
                            for (int i = 0; i < AlarmModeList[a].Count; i++)
                            {
                                if (!(AlarmModeList[a][i] == "TEACH" || AlarmModeList[a][i] == "REMOTE(TEACH)" || AlarmModeList[a][i] == "REMOTE(PLAY)" || AlarmModeList[a][i] == "PLAY" || AlarmModeList[a][i] == "REMOTE"))
                                {
                                    AlarmModeList[a][i] = " ";
                                }
                            }
                        }

                        // Yanıt olarak gönderilecek JSON verisi
                        var responseData_AlarmHist = new
                        {
                            type = "alarm",
                            data = new
                            {
                                type = "almhist",
                                ip_address = robotIP, // IP adresi diziden alınır
                                values = new List<Dictionary<dynamic, dynamic>>()
                            }
                        };

                        for (int a = 0; a < MAJOR_AlarmCode.Count; a++)
                        {
                            responseData_AlarmHist.data.values.Add(new Dictionary<dynamic, dynamic>
                                    {
                                        { "code",  MAJOR_AlarmCode[a]},
                                        { "type", "MAJOR" },
                                        { "name", MAJOR_AlarmName[a] },
                                        { "origin_date", MAJOR_AlarmTime[a] },
                                        { "mode", MAJOR_AlarmMode[a]}
                                    }
                            );
                        }

                        for (int a = 0; a < MINOR_AlarmCode.Count; a++)
                        {
                            responseData_AlarmHist.data.values.Add(new Dictionary<dynamic, dynamic>
                                    {
                                        { "code",  MINOR_AlarmCode[a]},
                                        { "type", "MINOR" },
                                        { "name", MINOR_AlarmName[a] },
                                        { "origin_date", MINOR_AlarmTime[a] },
                                        { "mode", MINOR_AlarmMode[a]}
                                    }
                            );
                        }

                        for (int a = 0; a < IO_SYS_AlarmCode.Count; a++)
                        {
                            responseData_AlarmHist.data.values.Add(new Dictionary<dynamic, dynamic>
                                    {
                                        { "code",  IO_SYS_AlarmCode[a]},
                                        { "type", "SYSTEM" },
                                        { "name", IO_SYS_AlarmName[a] },
                                        { "origin_date", IO_SYS_AlarmTime[a] },
                                        { "mode", IO_SYS_AlarmMode[a]}
                                    }
                            );
                        }

                        for (int a = 0; a < IO_USR_AlarmCode.Count; a++)
                        {
                            responseData_AlarmHist.data.values.Add(new Dictionary<dynamic, dynamic>
                                    {
                                        { "code",  IO_USR_AlarmCode[a]},
                                        { "type", "USER" },
                                        { "name", IO_USR_AlarmName[a] },
                                        { "origin_date", IO_USR_AlarmTime[a] },
                                        { "mode", IO_USR_AlarmMode[a]}
                                    }
                            );
                        }

                        for (int a = 0; a < OFFLINE_AlarmCode.Count; a++)
                        {
                            responseData_AlarmHist.data.values.Add(new Dictionary<dynamic, dynamic>
                                    {
                                        { "code",  OFFLINE_AlarmCode[a]},
                                        { "type", "OFF-LINE" },
                                        { "name", OFFLINE_AlarmName[a] },
                                        { "origin_date", OFFLINE_AlarmTime[a]},
                                        { "mode", OFFLINE_AlarmMode[a]}
                                    }
                            );
                        }

                        jsonData = JsonConvert.SerializeObject(responseData_AlarmHist, _jsonSettings);

                        if (JsonHistory != jsonData)
                        {
                            QueueConfig.EnqueueWithLimit(_AlarmHist_Queue, jsonData);
                            Interlocked.Increment(ref _AlarmHist_FetchCount);

                            JsonHistory = jsonData;

                            //Console.WriteLine($"{DateTime.Now} AlmHist Data: {jsonData}");
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.ALMHIST_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Alarm Geçmişi Hatası ({robotIP}): {ex.Message}");

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.ALMHIST_DELAY, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} AlarmHist görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    #endregion

    #region ROBOT STATUS
    private static async Task RobotStatus(string robotIP)
    {
        HSEClient.Status_Information StatusHitory = new Status_Information();
        bool StopSignal1 = false;
        bool StopSignal2 = false;
        bool StopSignal3 = false;
        bool hold1 = false;

        bool GlobalStop = false;

        bool DoorSignal = false;

        string taskKey = $"{robotIP}_RobotStatus";
        CancellationTokenSource cts = null;

        bool connection = false;
        bool connectionHistory = false;
        byte b = 1;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);
                    //await RobotConnection.CheckAndLogConnectionState(robotIP, robot);

                    if (RobotConnect)
                    {
                        HSEClient.Status_Information statusInfo;
                        robot.StatusInformationRead(out statusInfo);
                        byte robotStatus8002;
                        byte robotStatus5007;

                        robot.IORead(8002, out robotStatus8002);
                        robot.IORead(5007, out robotStatus5007);


                        if (!StatusHitory.Equals(statusInfo) || hold1 != ByteToBit(robotStatus5007, 1) || StopSignal1 != ByteToBitReverse(robotStatus8002, 6) || StopSignal2 != ByteToBitReverse(robotStatus8002, 5) || StopSignal3 != ByteToBitReverse(robotStatus8002, 7) || DoorSignal != ByteToBitReverse(robotStatus8002, 3))
                        {
                            StopSignal1 = ByteToBitReverse(robotStatus8002, 6);
                            StopSignal2 = ByteToBitReverse(robotStatus8002, 5);
                            StopSignal3 = ByteToBitReverse(robotStatus8002, 7);
                            hold1 = ByteToBit(robotStatus5007, 1);

                            if (StopSignal1 || StopSignal2 || StopSignal3)
                            {
                                GlobalStop = true;
                            }
                            else
                            {
                                GlobalStop = false;
                            }

                            var statusData = new
                            {
                                type = "robotStatus",
                                data = new
                                {
                                    ip_address = robotIP,
                                    values = new
                                    {
                                        teach = GetOperationMode(statusInfo),
                                        servo = statusInfo.ServoON,
                                        operating = statusInfo.Running,
                                        cycle = GetCycleMode(statusInfo),
                                        hold = hold1,
                                        alarm = statusInfo.Alarming,
                                        error = statusInfo.ErrorOccuring,
                                        stop = GlobalStop,
                                        door_opened = ByteToBitReverse(robotStatus8002, 3)
                                    }
                                }
                            };

                            var jsonData = JsonConvert.SerializeObject(statusData, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} Robot Status Data: {jsonData}");

                            QueueConfig.EnqueueWithLimit(_RobotStatus_Queue, jsonData);
                            Interlocked.Increment(ref _RobotStatus_FetchCount);

                            StatusHitory = statusInfo;
                            //StopSignal1 = ByteToBitReverse(robotStatus8002, 6);
                            //StopSignal2 = ByteToBitReverse(robotStatus8002, 5);
                            //StopSignal3 = ByteToBitReverse(robotStatus8002, 7);
                            DoorSignal = ByteToBitReverse(robotStatus8002, 3);

                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.STATUS_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} RobotStatus Hatası ({robotIP}): {ex.Message}");

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} RobotStatus görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }



    #endregion

    #region VARIABLE
    static byte?[] ValueHistoryByte = new byte?[DegiskenSayisi];
    static short?[] ValueHistoryIntager = new short?[DegiskenSayisi];
    static int?[] ValueHistoryDouble = new int?[DegiskenSayisi];
    static float?[] ValueHistoryReal = new float?[DegiskenSayisi];
    static string[] ValueHistoryString = Enumerable.Repeat("", DegiskenSayisi).ToArray();

    private static async Task Variable_Byte(string robotIP)
    {
        string taskKey = $"{robotIP}_Variable_Byte";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        byte?[] values = new byte?[DegiskenSayisi];
                        robot.B_Read(0, DegiskenSayisi, out values);

                        if (!ValueHistoryByte.SequenceEqual(values))
                        {
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (ValueHistoryByte[i] != values[i])
                                {
                                    var responseData = new
                                    {
                                        type = "variable",
                                        data = new
                                        {
                                            type = "b_read",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                                new
                                                {
                                                    no = i,
                                                    value = values[i],
                                                }
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    Console.WriteLine($"Variable Byte Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Variable_Byte_Queue, jsonData);
                                    Interlocked.Increment(ref _Variable_Byte_FetchCount);
                                }
                            }

                            ValueHistoryByte = values;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.VARIABLE_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Variable Byte Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Variable_Byte görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task Variable_Integer(string robotIP)
    {
        string taskKey = $"{robotIP}_Variable_Integer";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        short?[] values = new short?[DegiskenSayisi];
                        robot.I_Read(0, DegiskenSayisi, out values);

                        if (!ValueHistoryIntager.SequenceEqual(values))
                        {
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (ValueHistoryIntager[i] != values[i])
                                {
                                    var responseData = new
                                    {
                                        type = "variable",
                                        data = new
                                        {
                                            type = "i_read",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                                new
                                                {
                                                    no = i,
                                                    value = values[i],
                                                }
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    Console.WriteLine($"Variable Integer Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Variable_Integer_Queue, jsonData);
                                    Interlocked.Increment(ref _Variable_Integer_FetchCount);
                                }
                            }

                            ValueHistoryIntager = values;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.VARIABLE_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Variable Integer Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Variable_Integer görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task Variable_Double(string robotIP)
    {
        string taskKey = $"{robotIP}_Variable_Double";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        int?[] values = new int?[DegiskenSayisi];
                        robot.D_Read(0, DegiskenSayisi, out values);

                        if (!ValueHistoryDouble.SequenceEqual(values))
                        {
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (ValueHistoryDouble[i] != values[i])
                                {
                                    var responseData = new
                                    {
                                        type = "variable",
                                        data = new
                                        {
                                            type = "d_read",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                                new
                                                {
                                                    no = i,
                                                    value = values[i],
                                                }
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    Console.WriteLine($"Variable Double Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Variable_Double_Queue, jsonData);
                                    Interlocked.Increment(ref _Variable_Double_FetchCount);
                                }
                            }

                            ValueHistoryDouble = values;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.VARIABLE_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Variable Double Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Variable_Double görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task Variable_Real(string robotIP)
    {
        string taskKey = $"{robotIP}_Variable_Real";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        float?[] values = new float?[DegiskenSayisi];
                        robot.R_Read(0, DegiskenSayisi, out values);

                        if (!ValueHistoryReal.SequenceEqual(values))
                        {
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (ValueHistoryReal[i] != values[i])
                                {
                                    var responseData = new
                                    {
                                        type = "variable",
                                        data = new
                                        {
                                            type = "r_read",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                                new
                                                {
                                                    no = i,
                                                    value = values[i],
                                                }
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"Variable Real Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Variable_Real_Queue, jsonData);
                                    Interlocked.Increment(ref _Variable_Real_FetchCount);
                                }
                            }

                            ValueHistoryReal = values;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.VARIABLE_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Variable Real Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Variable_Real görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task Variable_String(string robotIP)
    {
        string taskKey = $"{robotIP}_Variable_String";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        string[] strings1, strings2, strings3, strings4;

                        robot.S_Read(0, 25, out strings1);
                        robot.S_Read(25, 25, out strings2);
                        robot.S_Read(50, 25, out strings3);
                        robot.S_Read(75, 25, out strings4);

                        string[] combinedArray = strings1.Concat(strings2).Concat(strings3).Concat(strings4).ToArray();

                        if (!ValueHistoryString.SequenceEqual(combinedArray))
                        {
                            for (int i = 0; i < combinedArray.Length; i++)
                            {
                                if (ValueHistoryString[i] != combinedArray[i])
                                {
                                    var responseData = new
                                    {
                                        type = "variable",
                                        data = new
                                        {
                                            type = "s_read",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                                new
                                                {
                                                    no = i,
                                                    value = combinedArray[i],
                                                }
                                            }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} Variable String Data: {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_Variable_String_Queue, jsonData);
                                    Interlocked.Increment(ref _Variable_String_FetchCount);
                                }
                            }

                            ValueHistoryString = combinedArray;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.VARIABLE_STRING, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Variable String Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} Variable_String görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }

    #endregion

    #region REGISTER
    static int registerDegiskenSayisi = 1000;

    static ushort?[] HistoryRegister = new ushort?[registerDegiskenSayisi];

    private static async Task RegisterData(string robotIP)
    {
        string taskKey = $"{robotIP}_RegisterData";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        ushort?[] values1, values2, values3, values4, values5;

                        robot.RegRead(0, 225, out values1);
                        robot.RegRead(225, 225, out values2);
                        robot.RegRead(450, 225, out values3);
                        robot.RegRead(675, 225, out values4);
                        robot.RegRead(900, 100, out values5);


                        ushort?[] combinedArray = values1.Concat(values2).Concat(values3).Concat(values4).Concat(values5).ToArray();

                        if (!HistoryRegister.SequenceEqual(combinedArray))
                        {
                            // Değişen tüm değerleri topla
                            var changedValues = new List<object>();
                            
                            for (int i = 0; i < combinedArray.Length; i++)
                            {
                                if (HistoryRegister[i] != combinedArray[i])
                                {
                                    changedValues.Add(new
                                    {
                                        no = i,
                                        value = combinedArray[i],
                                    });
                                }
                            }

                            // Tüm değişiklikleri tek mesajda gönder
                            if (changedValues.Count > 0)
                            {
                                var responseData = new
                                {
                                    type = "register",
                                    data = new
                                    {
                                        type = "reg_read",
                                        ip_address = robotIP,
                                        values = changedValues.ToArray()
                                    }
                                };

                                var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                Console.WriteLine($"Register Data: {jsonData}");

                                QueueConfig.EnqueueWithLimit(_Register_Queue, jsonData);
                                Interlocked.Increment(ref _Register_FetchCount);
                            }

                            HistoryRegister = combinedArray;
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.REGISTER_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} Register Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} RegisterData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    #endregion

    #region MONITORING TASK MANAGEMENT
    private static void StopMonitoringTask(string taskKey, string taskName)
    {
        try
        {
            if (_robotTasks.TryRemove(taskKey, out var cts))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} {taskName} monitoring stopped - TaskKey: {taskKey}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                    Console.WriteLine($"{DateTime.Now} {taskName} CancellationTokenSource already disposed - TaskKey: {taskKey}");
                }
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} {taskName} task not found for stopping - TaskKey: {taskKey}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Error stopping {taskName} task - TaskKey: {taskKey}, Error: {ex.Message}");
        }
    }
    #endregion

    #region GET MANAGEMENT TIME
    private static async Task GetManagementTime(string robotIP)
    {
        string taskKey = $"{robotIP}_GetManagementTime";
        CancellationTokenSource cts = null;

        bool check = true;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        HSEClient.ManagementTime MT_Control_Power;
                        HSEClient.ManagementTime MT_Servo_Power;
                        HSEClient.ManagementTime MT_PlayBack_Time;
                        HSEClient.ManagementTime MT_Moving_Time;

                        robot.GetManagementTime(1, out MT_Control_Power);
                        robot.GetManagementTime(10, out MT_Servo_Power);
                        robot.GetManagementTime(110, out MT_PlayBack_Time);
                        robot.GetManagementTime(210, out MT_Moving_Time);

                        string CPower = MT_Control_Power.ElapsedTime;
                        string SPower = MT_Servo_Power.ElapsedTime;
                        string PTime = MT_PlayBack_Time.ElapsedTime;
                        string MTime = MT_Moving_Time.ElapsedTime;

                        string C = CPower.Substring(0, CPower.IndexOf(':'));
                        string S = SPower.Substring(0, SPower.IndexOf(':'));
                        string P = PTime.Substring(0, PTime.IndexOf(':'));
                        string M = MTime.Substring(0, MTime.IndexOf(':'));

                        UtilizationDB utilizationDB = new UtilizationDB(robotIP);

                        if (!(utilizationDB.UtilizationList.control_power_time.ToString() == C && utilizationDB.UtilizationList.servo_power_time.ToString() == S && utilizationDB.UtilizationList.playback_time.ToString() == P && utilizationDB.UtilizationList.moving_time.ToString() == M))
                        {
                            var responseData = new
                            {
                                type = "utilization",
                                data = new
                                {
                                    ip_address = robotIP,
                                    values = new[]
                                    {
                                        new
                                        {
                                            control_power_time = C,
                                            servo_power_time = S,
                                            playback_time = P,
                                            moving_time =  M
                                        }
                                    }
                                }
                            };

                            var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} GetManagementTime Data: {jsonData}");

                            QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
                            Interlocked.Increment(ref _GetManagementTime_FetchCount);
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.GET_MANAGEMENT_TIME_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} GetManagementTime Hatası ({robotIP}): {ex.Message}");

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.GET_MANAGEMENT_TIME_DELAY, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} GetManagementTime görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }


    #endregion

    #region JOB

    private static async Task JobSelectData(string robotIP, string messageType)
    {
        string taskKey = $"{robotIP}_JobSelectData";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            Console.WriteLine($"{DateTime.Now} JobSelectData görevi başlatıldı - IP: {robotIP}, Type: {messageType}");

            try
            {
                var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                if (RobotConnect)
                {
                    if (messageType == "getJobList")
                    {
                        // Get job list from robot
                        List<string> jobList;
                        robot.FileList("*.JBI", out jobList);

                        Console.WriteLine($"{DateTime.Now} Robot {robotIP} - {jobList.Count} job bulundu");

                        var jobSelectData = new
                        {
                            type = "jobSelect",
                            data = new
                            {
                                type = "jobList",
                                ip_address = robotIP,
                                values = jobList.Select((jobName, index) => new
                                {
                                    id = $"{robotIP}_{jobName}_{index}",
                                    name = jobName.Replace(".JBI", ""),
                                    filename = jobName,
                                    created_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                }).ToArray()
                            }
                        };

                        var jsonData = JsonConvert.SerializeObject(jobSelectData, _jsonSettings);
                        Console.WriteLine($"{DateTime.Now} JobSelectData: {jsonData}");

                        QueueConfig.EnqueueWithLimit(_TorkExamJobList_Queue, jsonData);
                        Interlocked.Increment(ref _TorkExamJobList_FetchCount);
                    }
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now} Robot {robotIP} bağlantısı sağlanamadı - JobSelectData");
                }
            }
            catch (OperationCanceledException)
            {
                // Token cancelled, exit gracefully
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"{DateTime.Now} JobSelectData Hatası ({robotIP}): {ex.Message}");
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} JobSelectData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }

    private static async Task JobData(string robotIP)
    {
        string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
        string JobDataBasePath = Path.Combine(watchlogDirectory, "JobData");

        // Ana JobData klasörünü oluştur
        if (!Directory.Exists(JobDataBasePath))
        {
            Directory.CreateDirectory(JobDataBasePath);
            Console.WriteLine($"{DateTime.Now} JobData klasörü oluşturuldu: {JobDataBasePath}");
        }

        string JobNameHistory = "";
        string JobDataIcerigi = "";

        uint JobDataLineNumberHistory = 0;

        string taskKey = $"{robotIP}_JobData";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    HSEClient.ExecutingJobInfo executingJobInfo;
                    HSEClient.Status_Information status_Information;

                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        robot.StatusInformationRead(out status_Information);

                        if (status_Information.Teach)
                        {
                            JobNameHistory = "";
                            JobDataIcerigi = "";
                        }

                        if (status_Information.Running == true)
                        {
                            robot.ExecutingJobInformationRead(1, out executingJobInfo);

                            if (JobNameHistory != executingJobInfo.JobName)
                            {
                                robot.FileSave(executingJobInfo.JobName + ".JBI", JobDataBasePath);
                                //c.Files.SaveFromControllerToString(executingJobInfo.JobName + ".JBI", out JobDataBasePath);

                                Console.WriteLine($"Dosya indirildi : {executingJobInfo.JobName}");

                                JobDataIcerigi = File.ReadAllText(JobDataBasePath + $"\\{executingJobInfo.JobName}.JBI");
                            }

                            JobNameHistory = executingJobInfo.JobName;

                            if (JobDataLineNumberHistory != executingJobInfo.LineNo)
                            {
                                var responseData = new
                                {
                                    type = "job",
                                    data = new
                                    {
                                        type = "job",
                                        ip_address = robotIP,
                                        values = new[]
                                        {
                                            new
                                            {
                                                job_name = executingJobInfo.JobName,
                                                current_line = executingJobInfo.LineNo,
                                                job_content = JobDataIcerigi
                                            }
                                        }
                                    }
                                };

                                var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                Console.WriteLine($"{DateTime.Now} JobData {jsonData}");

                                QueueConfig.EnqueueWithLimit(_JobData_Queue, jsonData);
                                Interlocked.Increment(ref _JobData_FetchCount);

                                JobDataLineNumberHistory = executingJobInfo.LineNo;
                            }
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.JOB_DATA_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} JOB DATA Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} JobData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }

            if (Directory.Exists(JobDataBasePath))
            {
                foreach (string file in Directory.GetFiles(JobDataBasePath))
                {
                    File.Delete(file);
                }
                Console.WriteLine($"{robotIP} JobData Klasördeki tüm dosyalar silindi.");
            }
            else
            {
                Console.WriteLine($"{robotIP} JobData Belirtilen klasör bulunamadı.");
            }
        }
    }

    #endregion

    #region TORK DEĞERLERİ
    private static async Task TorkData(string robotIP)
    {
        string taskKey = $"{robotIP}_TorkData";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        robot.TorqueDataRead(1, out int[] TorkData);

                        var responseData = new
                        {
                            type = "tork",
                            data = new
                            {
                                type = "tork",
                                ip_address = robotIP,
                                values = new[]
                                {
                                    new
                                    {
                                        S = Math.Abs(TorkData[0]),
                                        L = Math.Abs(TorkData[1]),
                                        U = Math.Abs(TorkData[2]),
                                        R = Math.Abs(TorkData[3]),
                                        B = Math.Abs(TorkData[4]),
                                        T = Math.Abs(TorkData[5])
                                    }
                                }
                            }
                        };

                        var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                        Console.WriteLine($"{DateTime.Now} TorkData {jsonData}");

                        QueueConfig.EnqueueWithLimit(_TorkData_Queue, jsonData);
                        Interlocked.Increment(ref _TorkData_FetchCount);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.TORK_DATA_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} TORK DATA Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} TorkData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }

    private static ConcurrentDictionary<string, CancellationTokenSource> _robotTorkExamTasks = new();

    private static List<(string Key, int Value)> HariciEksenler;
    private static byte[] AxisNo = { 11, 12, 13, 21, 22, 23, 24 };

    static int TorkExamTime_ms;
    static int TorkExamWhileCount;

    private static async Task TorkExam(string robotIP)
    {
        string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
        string TorkExamBasePath = Path.Combine(watchlogDirectory, "TorkExam");

        // Ana JobData klasörünü oluştur
        if (!Directory.Exists(TorkExamBasePath))
        {
            Directory.CreateDirectory(TorkExamBasePath);
            Console.WriteLine($"{DateTime.Now} TorkExam klasörü oluşturuldu: {TorkExamBasePath}");
        }

        string taskKey = $"{robotIP}_TorkExam";
        CancellationTokenSource cts = null;

        bool JobListOneTime = true;
        string JobSelectData = "";

        string JobDataIcerigi = "";

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        if (JobListOneTime)
                        {
                            robot.FileList("*.JBI", out List<string> JobList);

                            string[] JobListArray = JobList.ToArray();

                            var responseDataJobList = new
                            {
                                type = "torkExam",
                                data = new
                                {
                                    type = "jobList",
                                    ip_address = robotIP,
                                    values = new[]
                                    {
                                        new
                                        {
                                            jobList = JobListArray
                                        }
                                    }
                                }
                            };

                            var jsonDataJobList = JsonConvert.SerializeObject(responseDataJobList, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} TorkExam JOBLIST {jsonDataJobList}");

                            QueueConfig.EnqueueWithLimit(_TorkExamJobList_Queue, jsonDataJobList);
                            Interlocked.Increment(ref _TorkExamJobList_FetchCount);

                            robot.FileSave("SYSTEM.SYS", TorkExamBasePath);

                            HariciEksenTespit(TorkExamBasePath, out HariciEksenler);

                            /*
                            robot.TorqueDataRead(11, out int[] TorkData_B1);
                            robot.TorqueDataRead(12, out int[] TorkData_B2);
                            robot.TorqueDataRead(13, out int[] TorkData_B3);

                            robot.TorqueDataRead(21, out int[] TorkData_S1);
                            robot.TorqueDataRead(22, out int[] TorkData_S2);
                            robot.TorqueDataRead(23, out int[] TorkData_S3);
                            robot.TorqueDataRead(24, out int[] TorkData_S4);


                            int[] AxisArray = { TorkData_B1[0], TorkData_B2[0], TorkData_B3[0], TorkData_S1[0], TorkData_S2[0], TorkData_S3[0], TorkData_S4[0] };

                            for (int i = 0; i < AxisArray.Length; i++)
                            {
                                if (AxisArray[i] != 10001)
                                {
                                    OkeyAxis.Add(AxisNo[i]);
                                }
                            }
                            */
                            JobListOneTime = false;
                        }

                        if (WSMes_TorkExam_Type == "JobSelect" && JobSelectData != WSMes_TorkExam_SelectJob)
                        {
                            robot.FileSave(WSMes_TorkExam_SelectJob, TorkExamBasePath);

                            JobDataIcerigi = File.ReadAllText(TorkExamBasePath + $"\\{WSMes_TorkExam_SelectJob}");

                            var responseDataJobSelect = new
                            {
                                type = "torkExam",
                                data = new
                                {
                                    type = "job",
                                    ip_address = robotIP,
                                    values = new[]
                                    {
                                        new
                                        {
                                            job_name = WSMes_TorkExam_SelectJob,
                                            job_content = JobDataIcerigi,
                                            current_line = 0
                                        }
                                    }
                                }
                            };

                            var jsonDataJobSelect = JsonConvert.SerializeObject(responseDataJobSelect, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} TorkExam JOBSELECT {jsonDataJobSelect}");

                            QueueConfig.EnqueueWithLimit(_TorkExamJobSelect_Queue, jsonDataJobSelect);
                            Interlocked.Increment(ref _TorkExamJobSelect_FetchCount);

                            //JobSelectOneTime = false;
                            JobSelectData = WSMes_TorkExam_SelectJob;
                        }

                        if (WSMes_TorkExam_Type == "Start")
                        {
                            var cts_Start = new CancellationTokenSource();
                            var token_Start = cts_Start.Token;

                            var taskList = new List<Task>();

                            if (WSMes_TorkExam_SignalNo == null)
                            {
                                taskList.Add(Task.Run(() => TorkExamValue(robotIP, token_Start)));
                                taskList.Add(Task.Run(() => TorkExamJob(robotIP, TorkExamBasePath, token_Start)));
                            }
                            else
                            {
                                taskList.Add(Task.Run(() => TorkExamValue(robotIP, token_Start)));
                                taskList.Add(Task.Run(() => TorkExamSignal(robotIP, token_Start)));
                                taskList.Add(Task.Run(() => TorkExamJob(robotIP, TorkExamBasePath, token_Start)));
                            }

                            Console.WriteLine($"Görevler başlatıldı. {WSMes_TorkExam_Time} dakika bekleniyor...");

                            // Süre kadar bekle ve sonra token iptal et
                            try
                            {
                                await Task.Delay(TimeSpan.FromMinutes(Convert.ToDouble(WSMes_TorkExam_Time)), token_Start);
                            }
                            catch (TaskCanceledException)
                            {
                                Console.WriteLine("Bekleme süresi iptal edildi.");
                            }

                            // Süre dolduğunda
                            Console.WriteLine($"{WSMes_TorkExam_Time} dakika doldu. Görevler iptal ediliyor...    Ortalam çekme süresi = {TorkExamTime_ms / TorkExamWhileCount}ms , Toplam {TorkExamWhileCount} Veri çekildi.");
                            cts_Start.Cancel();

                            try
                            {
                                await Task.WhenAll(taskList);
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine($"Görevlerden biri iptal edildi.");
                            }
                            finally
                            {
                                cts_Start.Dispose();
                            }

                            Console.WriteLine("Tüm işlemler tamamlandı.");
                            WSMes_TorkExam_Type = "NULL";

                            /*
                            try
                            {
                                if (WSMes_TorkExam_SignalNo == null)
                                {
                                    await Task.WhenAll(TorkExamValue(robotIP), TorkExamJob(robotIP, TorkExamBasePath));
                                }
                                else
                                {
                                    await Task.WhenAll(TorkExamValue(robotIP), TorkExamSignal(robotIP), TorkExamJob(robotIP, TorkExamBasePath));
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Görevler iptal edildi.");
                            }

                            Console.WriteLine($"Görevler başlatıldı. {WSMes_TorkExam_Time} dakika bekleniyor...");

                            // 5 dakika bekle (300 saniye)
                            await Task.Delay(TimeSpan.FromMinutes(Convert.ToDouble(WSMes_TorkExam_Time)));

                            // CancellationToken tetikleniyor
                            Console.WriteLine($"{WSMes_TorkExam_Time} dakika doldu. Görevler iptal ediliyor...");


                            var existingTasks = _robotTorkExamTasks.Keys.ToList();

                            foreach (var existingTaskKey in existingTasks)
                            {
                                if (_robotTorkExamTasks.TryRemove(existingTaskKey, out var existingCts))
                                {
                                    existingCts.Cancel();
                                    existingCts.Dispose();
                                    //Console.WriteLine($"{DateTime.Now} Önceki görev durduruldu - IP: {message.data.ipAddress}, Task: {existingTaskKey}");
                                }
                            }

                            Console.WriteLine("Tüm işlemler tamamlandı.");
                            */
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.TORK_EXAM_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} TORK EXAM Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} TorkExam görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task TorkExamValue(string robotIP, CancellationToken token)
    {
        string taskKey = $"{robotIP}_TorkExamValue";
        CancellationTokenSource cts = null;
        //byte[] AxisNo = { 11, 12, 13, 21, 22, 23, 24 };

        dynamic values = new ExpandoObject();

        var dict = (IDictionary<string, object>)values;

        int[] TorqueData_B = new int[3];
        int[] TorqueData_S = new int[4];

        var jsonData = "";

        try
        {
            //cts = _robotTorkExamTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            //var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                Stopwatch sw = Stopwatch.StartNew(); // zamanlayıcı başlat

                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        robot.TorqueDataRead(1, out int[] TorqueData);

                        values.S = TorqueData[0];
                        values.L = TorqueData[1];
                        values.U = TorqueData[2];
                        values.R = TorqueData[3];
                        values.B = TorqueData[4];
                        values.T = TorqueData[5];

                        foreach (var Axis in HariciEksenler)
                        {
                            switch (Axis.Key)
                            {
                                case "S1":
                                    robot.TorqueDataRead(21, out TorqueData_S);

                                    for (int i = 0; i < Axis.Value; i++)
                                    {
                                        dict[$"S1_{i + 1}"] = TorqueData_S[i];
                                    }
                                    break;
                                case "S2":
                                    robot.TorqueDataRead(22, out TorqueData_S);

                                    for (int i = 0; i < Axis.Value; i++)
                                    {
                                        dict[$"S2_{i + 1}"] = TorqueData_S[i];
                                    }
                                    break;
                                case "S3":
                                    robot.TorqueDataRead(23, out TorqueData_S);

                                    for (int i = 0; i < Axis.Value; i++)
                                    {
                                        dict[$"S3_{i + 1}"] = TorqueData_S[i];
                                    }
                                    break;
                                case "B1":
                                    robot.TorqueDataRead(11, out TorqueData_B);

                                    for (int i = 0; i < Axis.Value; i++)
                                    {
                                        dict[$"B1_{i + 1}"] = TorqueData_B[i];
                                    }
                                    break;
                            }
                        }

                        /*
                        for (int i = 0; i < OkeyAxis.Count; i++)
                        {
                            switch (OkeyAxis[i])
                            {
                                case 11:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_B);
                                    values.B1 = TorqueData_B[0];
                                    break;
                                case 12:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_B);
                                    values.B2 = TorqueData_B[1];
                                    break;
                                case 13:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_B);
                                    values.B3 = TorqueData_B[2];
                                    break;
                                case 21:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_S);
                                    values.S1 = TorqueData_S[0];
                                    break;
                                case 22:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_S);
                                    values.S2 = TorqueData_S[0];
                                    break;
                                case 23:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_S);
                                    values.S3 = TorqueData_S[0];
                                    break;
                                case 24:
                                    robot.TorqueDataRead(OkeyAxis[i], out TorqueData_S);
                                    values.S4 = TorqueData_S[0];
                                    break;
                            }
                        }
                        */

                        /*
                        if (OkeyAxis[i])
                        {
                            robot.TorqueDataRead(AxisNo[i], out int[] TorqueData_S_B);

                            switch (AxisNo[i])
                            {
                                case 11:
                                    values.B1 = TorqueData_S_B[0];
                                    break;
                                case 21:
                                    values.S1 = TorqueData_S_B[0];
                                    break;
                                case 22:
                                    values.S1 = TorqueData_S_B[0];
                                    values.S2 = TorqueData_S_B[1];
                                    break;
                            }
                        }
                        */

                        var responseData = new

                        {
                            type = "torkExam",
                            data = new
                            {
                                type = "tork",
                                ip_address = robotIP,
                                values = values
                            }
                        };

                        jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);

                        //Console.WriteLine($"{DateTime.Now} TorkExam Tork {jsonData}  {sw.Elapsed.Milliseconds}ms");

                        QueueConfig.EnqueueWithLimit(_TorkExamTork_Queue, jsonData);
                        Interlocked.Increment(ref _TorkExamTork_FetchCount);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.TORK_EXAM_DELAY, token);
                    }
                    sw.Stop(); // zamanlayıcıyı durdur

                    TorkExamTime_ms = TorkExamTime_ms + sw.Elapsed.Milliseconds;
                    TorkExamWhileCount++;

                    Console.WriteLine($"{DateTime.Now} TorkExam Tork {jsonData}  {sw.Elapsed.Milliseconds}ms");

                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} TorkExamValue Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} TorkExamValue görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task TorkExamSignal(string robotIP, CancellationToken token)
    {
        string taskKey = $"{robotIP}_TorkExamSignal";
        CancellationTokenSource cts = null;

        bool[] SignalHistory = new bool[WSMes_TorkExam_SignalNo.Count];
        bool[] FirstTime = new bool[WSMes_TorkExam_SignalNo.Count];

        try
        {
            //cts = _robotTorkExamTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            //var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        for (int i = 0; i < WSMes_TorkExam_SignalNo.Count; i++)
                        {
                            int ByteSignal = Convert.ToInt32(WSMes_TorkExam_SignalNo[i].Substring(0, 4));
                            int bit = Convert.ToInt32(WSMes_TorkExam_SignalNo[i][4].ToString());

                            byte IO;
                            robot.IORead(ByteSignal, out IO);

                            bool Signal_Status = ByteToBit(IO, bit);


                            if (SignalHistory[i] != Signal_Status || !FirstTime[i])
                            {
                                var responseData = new
                                {
                                    type = "torkExam",
                                    data = new
                                    {
                                        type = "ioBit",
                                        ip_address = robotIP,
                                        values = new[]
                                        {
                                            new
                                            {
                                                bitNumber = WSMes_TorkExam_SignalNo[i],
                                                isActive = Signal_Status
                                            }
                                        }
                                    }
                                };

                                var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);

                                Console.WriteLine($"{DateTime.Now} TORKEXAM SIGNAL {jsonData}");

                                QueueConfig.EnqueueWithLimit(_TorkExamSignal_Queue, jsonData);
                                Interlocked.Increment(ref _TorkExamSignal_FetchCount);

                                SignalHistory[i] = Signal_Status;

                                FirstTime[i] = true;
                            }
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.TORK_EXAM_SIGNAL_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} TorkExamSignal Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} TorkExamSignal görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }
        }
    }
    private static async Task TorkExamJob(string robotIP, string JobDataBasePath, CancellationToken token)
    {
        string JobNameHistory = "";
        string JobDataIcerigi = "";

        uint JobDataLineNumberHistory = 0;

        string taskKey = $"{robotIP}_TorkExamJob";
        CancellationTokenSource cts = null;

        try
        {
            //cts = _robotTorkExamTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            //var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    HSEClient.ExecutingJobInfo executingJobInfo;
                    HSEClient.Status_Information status_Information;

                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        robot.StatusInformationRead(out status_Information);

                        if (status_Information.Teach)
                        {
                            JobNameHistory = "";
                            JobDataIcerigi = "";
                        }

                        if (status_Information.Running == true)
                        {
                            robot.ExecutingJobInformationRead(1, out executingJobInfo);

                            if (WSMes_TorkExam_JobName == executingJobInfo.JobName + ".JBI")
                            {
                                if (JobNameHistory != executingJobInfo.JobName)
                                {
                                    robot.FileSave(executingJobInfo.JobName + ".JBI", JobDataBasePath);

                                    Console.WriteLine($"Dosya indirildi : {executingJobInfo.JobName}");

                                    JobDataIcerigi = File.ReadAllText(JobDataBasePath + $"\\{executingJobInfo.JobName}.JBI");
                                }

                                JobNameHistory = executingJobInfo.JobName;

                                if (JobDataLineNumberHistory != executingJobInfo.LineNo)
                                {
                                    var responseData = new
                                    {
                                        type = "torkExam",
                                        data = new
                                        {
                                            type = "job",
                                            ip_address = robotIP,
                                            values = new[]
                                            {
                                            new
                                            {
                                                job_name = executingJobInfo.JobName,
                                                current_line = executingJobInfo.LineNo,
                                                job_content = JobDataIcerigi
                                            }
                                        }
                                        }
                                    };

                                    var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                                    //Console.WriteLine($"{DateTime.Now} TorkExamJob {jsonData}");

                                    QueueConfig.EnqueueWithLimit(_TorkExamJob_Queue, jsonData);
                                    Interlocked.Increment(ref _TorkExamJob_FetchCount);

                                    JobDataLineNumberHistory = executingJobInfo.LineNo;
                                }
                            }
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.TORK_EXAM_JOB_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} JOB DATA Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(1000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} JobData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }

            if (Directory.Exists(JobDataBasePath))
            {
                foreach (string file in Directory.GetFiles(JobDataBasePath))
                {
                    File.Delete(file);
                }
                Console.WriteLine($"{robotIP} JobData Klasördeki tüm dosyalar silindi.");
            }
            else
            {
                Console.WriteLine($"{robotIP} JobData Belirtilen klasör bulunamadı.");
            }
        }
    }


    #region HARICI EKSEN TESPİT FONKSİYONU

    static void HariciEksenTespit(string FilePath, out List<(string Key, int Value)> results)
    {
        string filePath = FilePath + "\\SYSTEM.SYS";
        bool inTargetSection = false;
        var tempResults = new Dictionary<string, int>();
        results = new List<(string, int)>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.Contains("//ROBOT NAME"))
            {
                inTargetSection = true;
                continue;
            }

            if (line.Contains("//CONTROL POWER"))
            {
                inTargetSection = false;
                break;
            }

            if (inTargetSection)
            {
                ExtractValue("B1", line, tempResults);
                ExtractValue("S1", line, tempResults);
                ExtractValue("S2", line, tempResults);
                ExtractValue("S3", line, tempResults);
            }
        }

        // Dictionary içeriğini listeye taşı
        foreach (var item in tempResults)
        {
            results.Add((item.Key, item.Value));
        }
    }
    static void ExtractValue(string key, string line, Dictionary<string, int> results)
    {
        if (!line.Contains(key)) return;

        // Örnek eşleşme:
        // B1 : RECT-XY    0000_0011
        Match match = Regex.Match(line, $@"{key}\s*:?.*?([01_]+)");
        if (match.Success)
        {
            string binary = match.Groups[1].Value.Replace("_", "");
            int oneCount = CountOnes(binary);
            results[key] = oneCount;
        }
    }
    static int CountOnes(string binary)
    {
        int count = 0;
        foreach (char c in binary)
        {
            if (c == '1') count++;
        }
        return count;
    }

    #endregion

    #endregion

    #region ABSODATA

    static int[] AbsoDataHistory = new int[6];
    private static async Task AbsoData(string robotIP)
    {
        string watchlogDirectory = Path.Combine(AppContext.BaseDirectory, "Watchlog");
        string AbsoDataBasePath = Path.Combine(watchlogDirectory, "AbsoData");

        // Ana JobData klasörünü oluştur
        if (!Directory.Exists(AbsoDataBasePath))
        {
            Directory.CreateDirectory(AbsoDataBasePath);
            Console.WriteLine($"{DateTime.Now} AbsoData klasörü oluşturuldu: {AbsoDataBasePath}");
        }

        string AbsoDataIcerigi = "";


        string taskKey = $"{robotIP}_AbsoData";
        CancellationTokenSource cts = null;

        try
        {
            cts = _robotTasks.GetOrAdd(taskKey, _ => new CancellationTokenSource());
            var token = cts.Token;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var robot = RobotConnection.GetClient(robotIP, out bool RobotConnect);

                    if (RobotConnect)
                    {
                        robot.FileSave("ABSO.DAT", AbsoDataBasePath);
                        //Console.WriteLine($"Dosya indirildi : ABSO.DAT");

                        AbsoDataIcerigi = File.ReadAllText(AbsoDataBasePath + $"\\ABSO.DAT");

                        int[] AbsoDatas = ReadAbsoDAT(AbsoDataIcerigi);

                        if (!AbsoDataHistory.SequenceEqual(AbsoDatas))
                        {
                            var responseData = new
                            {
                                type = "absoData",
                                data = new
                                {
                                    type = "absoData",
                                    ip_address = robotIP,
                                    values = new[]
                                                  {
                                    new
                                    {
                                        S = AbsoDatas[0],
                                        L = AbsoDatas[1],
                                        U = AbsoDatas[2],
                                        R = AbsoDatas[3],
                                        B = AbsoDatas[4],
                                        T = AbsoDatas[5],
                                    }
                                }
                                }
                            };

                            var jsonData = JsonConvert.SerializeObject(responseData, _jsonSettings);
                            Console.WriteLine($"{DateTime.Now} AbsoData {jsonData}");

                            QueueConfig.EnqueueWithLimit(_AbsoData_Queue, jsonData);
                            Interlocked.Increment(ref _AbsoData_FetchCount);

                            AbsoDataHistory[0] = AbsoDatas[0];
                            AbsoDataHistory[1] = AbsoDatas[1];
                            AbsoDataHistory[2] = AbsoDatas[2];
                            AbsoDataHistory[3] = AbsoDatas[3];
                            AbsoDataHistory[4] = AbsoDatas[4];
                            AbsoDataHistory[5] = AbsoDatas[5];
                        }
                    }

                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(TaskDelays.ABSO_DATA_DELAY, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.WriteLine($"{DateTime.Now} ABSO DATA Hatası ({robotIP}): {ex.Message}");
                    if (!token.IsCancellationRequested)
                    {
                        await Task.Delay(30000, token);
                    }
                }
            }
        }
        finally
        {
            if (_robotTasks.TryRemove(taskKey, out _))
            {
                try
                {
                    if (cts != null && !cts.IsCancellationRequested)
                    {
                        cts.Cancel();
                    }
                    cts?.Dispose();
                    Console.WriteLine($"{DateTime.Now} AbsoData görevi temiz bir şekilde sonlandırıldı - IP: {robotIP}");
                }
                catch (ObjectDisposedException)
                {
                    // CancellationTokenSource zaten dispose edilmişse sessizce devam et
                }
            }

            /*
            if (Directory.Exists(AbsoDataBasePath))
            {
                foreach (string file in Directory.GetFiles(AbsoDataBasePath))
                {
                    File.Delete(file);
                }
                Console.WriteLine($"{robotIP} AbsoData Klasördeki tüm dosyalar silindi.");
            }
            else
            {
                Console.WriteLine($"{robotIP} AbsoData Belirtilen klasör bulunamadı.");
            }
            */
        }
    }

    #endregion

    #region HELP
    private static string GetOperationMode(HSEClient.Status_Information status)
    {
        if (status.Teach) return "TEACH";
        if (status.Play && !status.CommandRemote) return "PLAY";
        if (status.CommandRemote) return "REMOTE";
        return "UNKNOWN";
    }
    private static string GetCycleMode(HSEClient.Status_Information status)
    {
        if (status.Step) return "STEP";
        if (status.Cycle1) return "CYCLE";
        if (status.AutomaticAndContinous) return "AUTO";
        return "UNKNOWN";
    }
    private static bool ByteToBit(byte status, int bit)
    {
        return (status & (1 << bit)) != 0;
    }
    public static bool[] ByteToBoolArray(byte b)
    {
        bool[] bits = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            bits[i] = (b & (1 << i)) != 0;
        }
        return bits;
    }
    private static bool ByteToBitReverse(byte status, int bit)
    {
        return (status & (1 << bit)) == 0; // 6. bitin indeksi 5'tir (0'dan sayılır)
    }
    private static void ReadAlarmHist(string filePath, string StartAlarmType, out List<string> Alarmcode, out List<string> Alarmname, out List<string> AlarmTime, out List<string> AlarmMode)
    {
        string EndAlarmType = "";

        if (StartAlarmType == "///MAJOR")
        {
            EndAlarmType = "///MINOR";
        }
        else if (StartAlarmType == "///MINOR")
        {
            EndAlarmType = "///IO_SYS";
        }
        else if (StartAlarmType == "///IO_SYS")
        {
            EndAlarmType = "///IO_USR";
        }
        else if (StartAlarmType == "///IO_USR")
        {
            EndAlarmType = "///OFFLINE";
        }
        else if (StartAlarmType == "///OFFLINE")
        {
            EndAlarmType = "0";
        }

        List<string> extractedText = new List<string>();

        // Read all lines from the file
        string[] lines = File.ReadAllLines(filePath);

        bool isExtracting = false;
        List<string> currentParagraph = new List<string>();

        foreach (string line in lines)
        {
            if (line.Contains(StartAlarmType))
            {
                isExtracting = true;
                continue; // Skip the "///MINOR" line itself
            }

            if (EndAlarmType != "0")
            {
                if (line.Contains(EndAlarmType))
                {
                    if (currentParagraph.Count > 0)
                    {
                        extractedText.Add(string.Join(Environment.NewLine, currentParagraph));
                    }
                    break; // Stop extraction when "///IO_SYS" is found
                }
            }

            if (isExtracting)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // If there's an empty line, it indicates the end of a paragraph
                    if (currentParagraph.Count > 0)
                    {
                        extractedText.Add(string.Join(Environment.NewLine, currentParagraph));
                        currentParagraph.Clear(); // Start a new paragraph
                    }
                }
                else
                {
                    currentParagraph.Add(line.Trim());
                }
            }
        }

        Alarmcode = new List<string>();
        Alarmname = new List<string>();
        AlarmTime = new List<string>();
        AlarmMode = new List<string>();

        string dateTimePattern = @"\d{4}/\d{2}/\d{2} \d{2}:\d{2}(:\d{2})?";

        foreach (var alarm in extractedText)
        {
            // Virgül ile ayırmak için Split kullanıyoruz
            var parts = alarm.Split(',');

            // Alarmcode, Alarmname ve alarmTime dizilerine ekleme
            if (parts.Length >= 3)
            {
                // Son 4 karakteri almak
                string alarmCode = parts[0];
                if (alarmCode.Length > 4)
                {
                    alarmCode = alarmCode.Substring(alarmCode.Length - 4);
                }

                Alarmcode.Add(alarmCode);

                Alarmname.Add(parts[1]);         // İlk ve ikinci virgül arasındaki kısım

                AlarmMode.Add(parts[5]);

                // Son virgülden sonraki kısmı almak ve yalnızca tarih/saat kısmını eklemek
                string lastPart = alarm.Substring(alarm.LastIndexOf(',') + 1);

                // Tarih ve saat formatına uyan kısmı almak için Regex kullanımı
                Match match = Regex.Match(lastPart, dateTimePattern);
                if (match.Success)
                {
                    AlarmTime.Add(match.Value);  // Sadece tarih ve saat kısmını ekliyoruz
                }
            }
        }
    }
    private static object CreateIOResponse(string robotIP, int startByte, byte data)
    {
        return new
        {
            type = "io",
            data = new
            {
                ip_address = robotIP,
                values = new[]
                         {
                             new
                             {
                                 byteNumber = startByte,
                                 bits = ByteToBoolArray(data),
                             }
                         }
            }
        };
    }
    private static async Task MonitorSystem(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            Console.WriteLine($"\n=== System Status Report === {DateTime.Now}");

            // Fetch, Send ve Queue durumlarını tek bir raporda göster
            Console.WriteLine("\n{0,-30} {1,15} {2,15} {3,15}", "Data Type", "Fetch/10sec", "Send/10sec", "Queue Size");
            Console.WriteLine(new string('-', 75));

            ReportMetrics("Robot Status", _RobotStatus_FetchCount, _RobotStatus_SendCount, _RobotStatus_Queue);
            ReportMetrics("Alarm", _Alarm_FetchCount, _Alarm_SendCount, _Alarm_Queue);
            ReportMetrics("Alarm History", _AlarmHist_FetchCount, _AlarmHist_SendCount, _AlarmHist_Queue);
            ReportMetrics("IO General Input", _IO_General_Input_FetchCount, _IO_General_Input_SendCount, _IO_General_Input_Queue);
            ReportMetrics("IO General Output", _IO_General_Output_FetchCount, _IO_General_Output_SendCount, _IO_General_Output_Queue);
            ReportMetrics("IO External Input", _IO_External_Input_FetchCount, _IO_External_Input_SendCount, _IO_External_Input_Queue);
            ReportMetrics("IO External Output", _IO_External_Output_FetchCount, _IO_External_Output_SendCount, _IO_External_Output_Queue);
            ReportMetrics("IO Network Input", _IO_Network_Input_FetchCount, _IO_Network_Input_SendCount, _IO_Network_Input_Queue);
            ReportMetrics("IO Network Output", _IO_Network_Output_FetchCount, _IO_Network_Output_SendCount, _IO_Network_Output_Queue);
            ReportMetrics("IO Specific Input", _IO_Specific_Input_FetchCount, _IO_Specific_Input_SendCount, _IO_Specific_Input_Queue);
            ReportMetrics("IO Specific Output", _IO_Specific_Output_FetchCount, _IO_Specific_Output_SendCount, _IO_Specific_Output_Queue);
            ReportMetrics("IO Auxiliary Relay", _IO_AuxiliaryRelay_FetchCount, _IO_AuxiliaryRelay_SendCount, _IO_AuxiliaryRelay_Queue);
            ReportMetrics("IO Internal Control", _IO_Internal_Control_Status_FetchCount, _IO_Internal_Control_Status_SendCount, _IO_Internal_Control_Status_Queue);
            ReportMetrics("IO Pseudo Input", _IO_Pseudo_Input_FetchCount, _IO_Pseudo_Input_SendCount, _IO_Pseudo_Input_Queue);
            ReportMetrics("Byte Variables", _Variable_Byte_FetchCount, _Variable_Byte_SendCount, _Variable_Byte_Queue);
            ReportMetrics("Integer Variables", _Variable_Integer_FetchCount, _Variable_Integer_SendCount, _Variable_Integer_Queue);
            ReportMetrics("Double Variables", _Variable_Double_FetchCount, _Variable_Double_SendCount, _Variable_Double_Queue);
            ReportMetrics("Real Variables", _Variable_Real_FetchCount, _Variable_Real_SendCount, _Variable_Real_Queue);
            ReportMetrics("String Variables", _Variable_String_FetchCount, _Variable_String_SendCount, _Variable_String_Queue);
            ReportMetrics("Management Time", _GetManagementTime_FetchCount, _GetManagementTime_SendCount, _GetManagementTime_Queue);
            ReportMetrics("Job Data", _JobData_FetchCount, _JobData_SendCount, _JobData_Queue);
            ReportMetrics("Tork Data", _TorkData_FetchCount, _TorkData_SendCount, _TorkData_Queue);
            ReportMetrics("Abso Data", _AbsoData_FetchCount, _AbsoData_SendCount, _AbsoData_Queue);



            // Sayaçları sıfırla
            Interlocked.Exchange(ref _RobotStatus_FetchCount, 0);
            Interlocked.Exchange(ref _RobotStatus_SendCount, 0);
            Interlocked.Exchange(ref _Alarm_FetchCount, 0);
            Interlocked.Exchange(ref _Alarm_SendCount, 0);
            Interlocked.Exchange(ref _AlarmHist_FetchCount, 0);
            Interlocked.Exchange(ref _AlarmHist_SendCount, 0);
            Interlocked.Exchange(ref _IO_General_Input_FetchCount, 0);
            Interlocked.Exchange(ref _IO_General_Input_SendCount, 0);
            Interlocked.Exchange(ref _IO_General_Output_FetchCount, 0);
            Interlocked.Exchange(ref _IO_General_Output_SendCount, 0);
            Interlocked.Exchange(ref _IO_External_Input_FetchCount, 0);
            Interlocked.Exchange(ref _IO_External_Input_SendCount, 0);
            Interlocked.Exchange(ref _IO_External_Output_FetchCount, 0);
            Interlocked.Exchange(ref _IO_External_Output_SendCount, 0);
            Interlocked.Exchange(ref _IO_Network_Input_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Network_Input_SendCount, 0);
            Interlocked.Exchange(ref _IO_Network_Output_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Network_Output_SendCount, 0);
            Interlocked.Exchange(ref _IO_Specific_Input_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Specific_Input_SendCount, 0);
            Interlocked.Exchange(ref _IO_Specific_Output_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Specific_Output_SendCount, 0);
            Interlocked.Exchange(ref _IO_AuxiliaryRelay_FetchCount, 0);
            Interlocked.Exchange(ref _IO_AuxiliaryRelay_SendCount, 0);
            Interlocked.Exchange(ref _IO_Internal_Control_Status_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Internal_Control_Status_SendCount, 0);
            Interlocked.Exchange(ref _IO_Pseudo_Input_FetchCount, 0);
            Interlocked.Exchange(ref _IO_Pseudo_Input_SendCount, 0);
            Interlocked.Exchange(ref _Variable_Byte_FetchCount, 0);
            Interlocked.Exchange(ref _Variable_Byte_SendCount, 0);
            Interlocked.Exchange(ref _Variable_Integer_FetchCount, 0);
            Interlocked.Exchange(ref _Variable_Integer_SendCount, 0);
            Interlocked.Exchange(ref _Variable_Double_FetchCount, 0);
            Interlocked.Exchange(ref _Variable_Double_SendCount, 0);
            Interlocked.Exchange(ref _Variable_Real_FetchCount, 0);
            Interlocked.Exchange(ref _Variable_Real_SendCount, 0);
            Interlocked.Exchange(ref _Variable_String_FetchCount, 0);
            Interlocked.Exchange(ref _Variable_String_SendCount, 0);
            Interlocked.Exchange(ref _GetManagementTime_FetchCount, 0);
            Interlocked.Exchange(ref _GetManagementTime_SendCount, 0);
            Interlocked.Exchange(ref _JobData_FetchCount, 0);
            Interlocked.Exchange(ref _JobData_SendCount, 0);
            Interlocked.Exchange(ref _TorkData_FetchCount, 0);
            Interlocked.Exchange(ref _TorkData_SendCount, 0);
            Interlocked.Exchange(ref _AbsoData_FetchCount, 0);
            Interlocked.Exchange(ref _AbsoData_SendCount, 0);

            Interlocked.Exchange(ref _Register_SendCount, 0);


            Console.WriteLine($"\n===========================\n {DateTime.Now}");

            await Task.Delay(TaskDelays.MONITOR_DELAY, ct);
        }
    }
    private static void ReportMetrics(string name, int fetchCount, int sendCount, ConcurrentQueue<string> queue)
    {
        Console.WriteLine("{0,-30} {1,15} {2,15} {3,15}", name, fetchCount, sendCount, queue.Count);
    }
    private static int[] ReadAbsoDAT(string fileContent)
    {
        // Satırlara ayır
        string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // 3. ve 4. satırları al (index 2 ve 3)
        string line3 = lines[2];
        string line4 = lines[3];

        // Her satırdaki sayıları array'e çevir
        string[] line3Parts = line3.Split(',');
        string[] line4Parts = line4.Split(',');

        // "1," sonrası sayıları alacak liste
        var numbers = new System.Collections.Generic.List<int>();

        // 3. satırdan "1," sonrası sayıları ekle
        for (int i = 0; i < line3Parts.Length; i++)
        {
            if (line3Parts[i] == "1" && i + 1 < line3Parts.Length)
            {
                if (int.TryParse(line3Parts[i + 1], out int number))
                {
                    numbers.Add(number);
                }
            }
        }

        // 4. satırdan "1," sonrası sayıları ekle
        for (int i = 0; i < line4Parts.Length; i++)
        {
            if (line4Parts[i] == "1" && i + 1 < line4Parts.Length)
            {
                if (int.TryParse(line4Parts[i + 1], out int number))
                {
                    numbers.Add(number);
                }
            }
        }

        // Sonucu array'e çevir
        int[] resultArray = numbers.ToArray();

        return resultArray;
    }

    private static async Task ManualBackupData(string ipAddress, string controllerName, List<string> fileTypes, string requestId, string controllerId)
    {
        
        if (!await _manualBackupSemaphore.WaitAsync(1000))
        {
            Console.WriteLine($"{DateTime.Now} Manual backup reddedildi - sistem meşgul: {ipAddress}");
            
            var busyResponse = new
            {
                type = "manualBackupComplete",
                data = new
                {
                    requestId = requestId,
                    success = false,
                    error = "Sistem meşgul. Şu anda maksimum sayıda backup işlemi çalışıyor. Lütfen birkaç dakika bekleyip tekrar deneyin.",
                    controllerName = controllerName,
                    ipAddress = ipAddress,
                    controllerId = controllerId
                }
            };
            
            var jsonData = JsonConvert.SerializeObject(busyResponse, _jsonSettings);
            QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
            Interlocked.Increment(ref _GetManagementTime_FetchCount);
            return;
        }
        
        try
        {
            Console.WriteLine($"{DateTime.Now} Manual backup başlatıldı - IP: {ipAddress}, Controller: {controllerName}");

            // Temporary directory for backup files
            string tempDirectory = Path.Combine(Path.GetTempPath(), $"manual_backup_{requestId}");
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);

            var now = DateTime.Now;
            List<List<string>> fileLists = new List<List<string>>();
            bool CmosTarget = false;
            List<string> backupFiles = new List<string>();

            MotomanController c = MotomanController.OpenConnection(ipAddress, out StatusInfo status);

            if (status.ToString() == "Code (0): OK")
            {
                Console.WriteLine($"{DateTime.Now} {ipAddress} Manual backup - Controller bağlantısı başarılı");

                // Get file lists based on requested file types
                Console.WriteLine($"{DateTime.Now} {ipAddress} Dosya tipleri işleniyor: {string.Join(", ", fileTypes)}");
                foreach (var fileType in fileTypes)
                {
                    switch (fileType.ToLower())
                    {
                        case "jbi":
                        case ".jbi":
                            Console.WriteLine($"{DateTime.Now} {ipAddress} JBI dosyaları listeleniyor...");
                            c.Files.ListFiles(FileType.Job_JBI, out List<string> fileList1, true);
                            Console.WriteLine($"{DateTime.Now} {ipAddress} {fileList1.Count} JBI dosyası bulundu");
                            fileLists.Add(fileList1);
                            break;
                        case "dat":
                        case ".dat":
                            Console.WriteLine($"{DateTime.Now} {ipAddress} DAT dosyaları listeleniyor...");
                            c.Files.ListFiles(FileType.Data_DAT, out List<string> fileList2, true);
                            Console.WriteLine($"{DateTime.Now} {ipAddress} {fileList2.Count} DAT dosyası bulundu");
                            fileLists.Add(fileList2);
                            break;
                        case "cnd":
                        case ".cnd":
                            c.Files.ListFiles(FileType.Condition_CND, out List<string> fileList3, true);
                            fileLists.Add(fileList3);
                            break;
                        case "prm":
                        case ".prm":
                            c.Files.ListFiles(FileType.Parameter_PRM, out List<string> fileList4, true);
                            fileLists.Add(fileList4);
                            break;
                        case "sys":
                        case ".sys":
                            c.Files.ListFiles(FileType.System_SYS, out List<string> fileList5, true);
                            fileLists.Add(fileList5);
                            break;
                        case "lst":
                        case ".lst":
                            c.Files.ListFiles(FileType.Ladder_LST, out List<string> fileList6, true);
                            fileLists.Add(fileList6);
                            break;
                        case "log":
                        case ".log":
                            c.Files.ListFiles(FileType.Log_LOG, out List<string> fileList7, true);
                            fileLists.Add(fileList7);
                            break;
                        case "cmos":
                            Console.WriteLine($"{DateTime.Now} {ipAddress} CMOS dosyası işlenecek");
                            CmosTarget = true;
                            break;
                    }
                }

                // Download CMOS if requested
                if (CmosTarget)
                {
                    try
                    {
                        string username = "ftp";
                        string password = "";
                        string remoteFilePath = "/spdrv/CMOSBK.BIN";
                        string targetPath = Path.Combine(tempDirectory, "CMOS.BIN");

                        using (WebClient ftpClient = new WebClient())
                        {
                            ftpClient.Credentials = new NetworkCredential(username, password);
                            string ftpPath = $"ftp://{ipAddress}{remoteFilePath}";
                            ftpClient.DownloadFile(ftpPath, targetPath);
                            backupFiles.Add(targetPath);
                            Console.WriteLine($"{DateTime.Now} {ipAddress} CMOS dosyası başarıyla indirildi.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.Now} {ipAddress} CMOS indirme hatası: {ex.Message}");
                    }
                }

                // Download other files
                int totalFiles = fileLists.Sum(list => list.Count - 1);
                Console.WriteLine($"{DateTime.Now} {ipAddress} Toplam {totalFiles} dosya indirilecek...");
                
                
                if (totalFiles > 500)
                {
                    Console.WriteLine($"{DateTime.Now} {ipAddress} Çok fazla dosya: {totalFiles} (maksimum 500)");
                    
                    var tooManyFilesResponse = new
                    {
                        type = "manualBackupComplete",
                        data = new
                        {
                            requestId = requestId,
                            success = false,
                            error = $"Çok fazla dosya ({totalFiles}). Maksimum 500 dosya seçebilirsiniz. Lütfen daha az dosya tipi seçin.",
                            controllerName = controllerName,
                            ipAddress = ipAddress,
                            controllerId = controllerId
                        }
                    };
                    
                    var jsonData = JsonConvert.SerializeObject(tooManyFilesResponse, _jsonSettings);
                    QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
                    Interlocked.Increment(ref _GetManagementTime_FetchCount);
                    
                   
                    Directory.Delete(tempDirectory, true);
                    return;
                }
                
                for (int a = 0; a < fileLists.Count; a++)
                {
                    for (int i = 0; i < fileLists[a].Count - 1; i++)
                    {
                        try
                        {
                            status = c.Files.SaveFromControllerToString(fileLists[a][i], out string jobContents);

                            if (status.ToString() == "Code (0): OK")
                            {
                                string filePath = Path.Combine(tempDirectory, fileLists[a][i]);
                                File.WriteAllText(filePath, jobContents);
                                backupFiles.Add(filePath);
                                Console.WriteLine($"{DateTime.Now} {ipAddress} {fileLists[a][i]} başarıyla indirildi.");
                            }
                            else
                            {
                                Console.WriteLine($"{DateTime.Now} {ipAddress} {fileLists[a][i]} indirilemedi: {status}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now} {ipAddress} {fileLists[a][i]} indirme hatası: {ex.Message}");
                        }
                    }
                }

                // Create ZIP file
                if (backupFiles.Count > 0)
                {
                    Console.WriteLine($"{DateTime.Now} {ipAddress} ZIP dosyası oluşturuluyor - {backupFiles.Count} dosya...");
                    string zipFileName = $"{controllerName}_{ipAddress}_{now:yyyy-MM-dd_HH-mm-ss}.zip";
                    string zipPath = Path.Combine(Path.GetTempPath(), zipFileName);

                    using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        foreach (string filePath in backupFiles)
                        {
                            string fileName = Path.GetFileName(filePath);
                            archive.CreateEntryFromFile(filePath, fileName);
                        }
                    }

                    // ZIP boyut kontrolü
                    var zipInfo = new FileInfo(zipPath);
                    if (zipInfo.Length > 200 * 1024 * 1024) // 200MB limit
                    {
                        Console.WriteLine($"{DateTime.Now} {ipAddress} ZIP çok büyük: {zipInfo.Length / 1024 / 1024}MB");
                        
                        var tooBigResponse = new
                        {
                            type = "manualBackupComplete",
                            data = new
                            {
                                requestId = requestId,
                                success = false,
                                error = $"Yedek dosyası çok büyük ({zipInfo.Length / 1024 / 1024}MB). Maksimum 200MB olabilir. Lütfen daha az dosya seçin.",
                                controllerName = controllerName,
                                ipAddress = ipAddress,
                                controllerId = controllerId
                            }
                        };
                        
                        var tooBigJsonData = JsonConvert.SerializeObject(tooBigResponse, _jsonSettings);
                        QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, tooBigJsonData);
                        Interlocked.Increment(ref _GetManagementTime_FetchCount);
                        
                        // ZIP dosyasını sil ve çık
                        File.Delete(zipPath);
                        Directory.Delete(tempDirectory, true);
                        return;
                    }

                    // Read ZIP as Base64 and send via WebSocket - Memory-safe approach
                    string zipBase64;
                    using (var fileStream = new FileStream(zipPath, FileMode.Open))
                    {
                        if (fileStream.Length > 50 * 1024 * 1024) // 50MB memory limit
                        {
                            Console.WriteLine($"{DateTime.Now} {ipAddress} Dosya memory için çok büyük: {fileStream.Length / 1024 / 1024}MB");
                            
                            var memoryLimitResponse = new
                            {
                                type = "manualBackupComplete",
                                data = new
                                {
                                    requestId = requestId,
                                    success = false,
                                    error = $"Dosya memory için çok büyük ({fileStream.Length / 1024 / 1024}MB). Sistem güvenliği için maksimum 50MB memory'ye yüklenebilir.",
                                    controllerName = controllerName,
                                    ipAddress = ipAddress,
                                    controllerId = controllerId
                                }
                            };
                            
                            var memoryLimitJsonData = JsonConvert.SerializeObject(memoryLimitResponse, _jsonSettings);
                            QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, memoryLimitJsonData);
                            Interlocked.Increment(ref _GetManagementTime_FetchCount);
                            
                            // Cleanup and return
                            File.Delete(zipPath);
                            Directory.Delete(tempDirectory, true);
                            return;
                        }
                        
                        var bytes = new byte[fileStream.Length];
                        await fileStream.ReadAsync(bytes, 0, (int)fileStream.Length);
                        zipBase64 = Convert.ToBase64String(bytes);
                    }

                    var backupResponse = new
                    {
                        type = "manualBackupComplete",
                        data = new
                        {
                            requestId = requestId,
                            success = true,
                            fileName = zipFileName,
                            fileData = zipBase64,
                            fileCount = backupFiles.Count,
                            controllerName = controllerName,
                            ipAddress = ipAddress,
                            controllerId = controllerId
                        }
                    };

                    var jsonData = JsonConvert.SerializeObject(backupResponse, _jsonSettings);
                    Console.WriteLine($"{DateTime.Now} Manual backup tamamlandı - {backupFiles.Count} dosya, ZIP boyutu: {new FileInfo(zipPath).Length} bytes");

                    
                    Console.WriteLine($"{DateTime.Now} {ipAddress} Backup sonucu WebSocket'e gönderiliyor - RequestId: {requestId}");
                    QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
                    Interlocked.Increment(ref _GetManagementTime_FetchCount);

                    
                    File.Delete(zipPath);
                }
                else
                {
                    
                    var errorResponse = new
                    {
                        type = "manualBackupComplete",
                        data = new
                        {
                            requestId = requestId,
                            success = false,
                            error = "No files found to backup",
                            controllerName = controllerName,
                            ipAddress = ipAddress,
                            controllerId = controllerId
                        }
                    };

                    var jsonData = JsonConvert.SerializeObject(errorResponse, _jsonSettings);
                    QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
                    Interlocked.Increment(ref _GetManagementTime_FetchCount);
                }

                
                Directory.Delete(tempDirectory, true);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now} {ipAddress} Manual backup - Controller bağlantısı başarısız: {status}");
                
                
                var errorResponse = new
                {
                    type = "manualBackupComplete",
                    data = new
                    {
                        requestId = requestId,
                        success = false,
                        error = $"Controller connection failed: {status}",
                        controllerName = controllerName,
                        ipAddress = ipAddress,
                        controllerId = controllerId
                    }
                };

                var jsonData = JsonConvert.SerializeObject(errorResponse, _jsonSettings);
                QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
                Interlocked.Increment(ref _GetManagementTime_FetchCount);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{DateTime.Now} Manual backup hatası: {ex.Message}");
            
            
            var errorResponse = new
            {
                type = "manualBackupComplete",
                data = new
                {
                    requestId = requestId,
                    success = false,
                    error = ex.Message,
                    controllerName = controllerName,
                    ipAddress = ipAddress,
                    controllerId = controllerId
                }
            };

            var jsonData = JsonConvert.SerializeObject(errorResponse, _jsonSettings);
            QueueConfig.EnqueueWithLimit(_GetManagementTime_Queue, jsonData);
            Interlocked.Increment(ref _GetManagementTime_FetchCount);
        }
        finally
        {
            _manualBackupSemaphore.Release();
        }
    }

    #endregion
}

