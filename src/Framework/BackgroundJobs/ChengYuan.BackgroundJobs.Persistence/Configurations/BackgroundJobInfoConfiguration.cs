using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChengYuan.BackgroundJobs;

public sealed class BackgroundJobInfoConfiguration : IEntityTypeConfiguration<BackgroundJobInfo>
{
    public void Configure(EntityTypeBuilder<BackgroundJobInfo> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Id)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(job => job.JobName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(job => job.JobArgs)
            .IsRequired();

        builder.Property(job => job.Priority)
            .IsRequired();

        builder.Property(job => job.TryCount)
            .IsRequired();

        builder.Property(job => job.CreationTime)
            .IsRequired();

        builder.Property(job => job.NextTryTime)
            .IsRequired();

        builder.HasIndex(job => new { job.IsAbandoned, job.NextTryTime })
            .HasDatabaseName("IX_BackgroundJob_Waiting");
    }
}
