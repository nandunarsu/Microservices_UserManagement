namespace AdminManagement.Interface
{
    public interface IDepartment
    {
        public Task<int> CreateDepartMent(string name);
    }
}
