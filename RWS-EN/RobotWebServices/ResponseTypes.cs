using System.Text.Json.Serialization;

namespace RWS_EN.RobotWebServices
{
    #region CTRL

    public class ControllerInfo
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("datetime")]
        public string Datetime { get; set; }

        [JsonPropertyName("ctrl-name")]
        public string Name { get; set; }

        [JsonPropertyName("ctrl-type")]
        public string CtrlType { get; set; }
    }

    #endregion

    #region RW/SYSTEM
    public class SystemProperties
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rwversion")]
        public string RWVersion { get; set; }

        [JsonPropertyName("rwversionname")]
        public string RWVersionName { get; set; }

        [JsonPropertyName("options")]
        public List<SystemOptions> Options { get; set; }
    }
    public class SystemOptions
    {
        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("option")]
        public string Option { get; set; }
    }

    #endregion

    #region RW/IO

    public class IOSignal
    {
        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } // DI, DO, AI, AO

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("lvalue")]
        public string Value { get; set; } // Difference here

        [JsonPropertyName("lstate")]
        public string State { get; set; }

        [JsonPropertyName("unitnm")]
        public string Unit { get; set; }
    }

    public class _IOSignal
    {
        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } // DI, DO, AI, AO

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("lvalue")]
        public int Value { get; set; } // Difference here

        [JsonPropertyName("lstate")]
        public string State { get; set; }

        [JsonPropertyName("unitnm")]
        public string Unit { get; set; }
    }
    #endregion

    #region RAPID
    public class RAPIDTask
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("taskstate")]
        public string State { get; set; }

        [JsonPropertyName("excstate")]
        public string Excstate { get; set; }

        [JsonPropertyName("active")]
        public string Active { get; set; }

        [JsonPropertyName("motiontask")]
        public string MotionTask { get; set; }
    }

    public class RAPIDTaskState
    {
        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("taskstate")]
        public string Taskstate { get; set; }

        [JsonPropertyName("excstate")]
        public string Excstate { get; set; }

        [JsonPropertyName("active")]
        public string Active { get; set; }

        [JsonPropertyName("motiontask")]
        public string Motiontask { get; set; }

        [JsonPropertyName("tasktype")]
        public string Tasktype { get; set; }

        [JsonPropertyName("trust")]
        public string Trust { get; set; }

        [JsonPropertyName("taskID")]
        public string TaskID { get; set; }

        [JsonPropertyName("execmode")]
        public string Execmode { get; set; }

        [JsonPropertyName("exctype")]
        public string Exctype { get; set; }

        [JsonPropertyName("prodentrypt")]
        public string Prodentrypt { get; set; }

        [JsonPropertyName("bind_ref")]
        public string Bind_ref { get; set; }

        [JsonPropertyName("task_in_forgnd")]
        public string Task_in_forgnd { get; set; }
    }

    public class RAPIDExecutionState
    {
        [JsonPropertyName("ctrlexecstate")]
        public string ControllerState { get; set; }

        //[JsonPropertyName("curtask")]
        //public string CurrentTask { get; set; }

        //[JsonPropertyName("execstate")]
        //public string ExecutionState { get; set; }

        [JsonPropertyName("cycle")]
        public string Cycle { get; set; }
    }

    public class RAPIDVariable
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("_type")]
        public string Type { get; set; }

        [JsonPropertyName("_title")]
        public string Title { get; set; }
    }
    #endregion

    #region FILESYSTEM
    public class FileSystemItem
    {
        [JsonPropertyName("_title")]
        public string Title { get; set; }

        [JsonPropertyName("_type")]
        public string Type { get; set; } // file or dir

        [JsonPropertyName("fs-cdate")]
        public string Cdate { get; set; }

        [JsonPropertyName("fs-mdate")]
        public string Mdate { get; set; }

        [JsonPropertyName("fs-size")]
        public string Size { get; set; }

        [JsonPropertyName("fs-readonly")]
        public string Readonly { get; set; }
    }
    #endregion
}