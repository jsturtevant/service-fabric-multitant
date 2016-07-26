namespace WorkService.Models
{
    public struct Job
    {
        public Job(string name, string parameters)
        {
            this.Name = name;
            this.Parameters = parameters;
        }

        public string Name { get; private set; }

        public string Parameters { get; private set; }

    }
}
