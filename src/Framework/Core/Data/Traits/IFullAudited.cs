namespace ChengYuan.Core.Data;

public interface IFullAudited : IAudited, ISoftDelete, IHasDeletionTime, IHasDeleterId;
