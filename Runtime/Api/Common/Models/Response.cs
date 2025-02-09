namespace RuntimeSounds.Api
{
    public class Response
    {
        public bool IsSuccess { get; set; } = true;

        public long Status { get; set; } = 200;

        public string Error { get; set; }
    }
}


