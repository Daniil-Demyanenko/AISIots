using AISIots.Models.DbTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AISIots.DAL;

public class RpdConfiguration: IEntityTypeConfiguration<Rpd>
{
    public void Configure(EntityTypeBuilder<Rpd> builder)
    {
        builder.HasMany(x => x.FosItog).WithOne().HasForeignKey("Rpd_FosItog");
        builder.HasMany(x => x.Fos).WithOne().HasForeignKey("Rpd_Fos");
        builder.HasMany(x => x.KursRab).WithOne().HasForeignKey("Rpd_KursRab");
        builder.HasMany(x => x.LecAnnotir).WithOne().HasForeignKey("Rpd_LecAnnotir");
        builder.HasMany(x => x.DopLitra).WithOne().HasForeignKey("Rpd_DopLitra");
        builder.HasMany(x => x.OsnLitra).WithOne().HasForeignKey("Rpd_OsnLitra");
        builder.HasMany(x => x.Nsr).WithOne().HasForeignKey("Rpd_Nsr");
        builder.HasMany(x => x.Nlab).WithOne().HasForeignKey("Rpd_Nlab");
        builder.HasMany(x => x.Npract).WithOne().HasForeignKey("Rpd_Npract");
        builder.HasMany(x => x.Nlec).WithOne().HasForeignKey("Rpd_Nlec");
        builder.HasMany(x => x.Zad).WithOne().HasForeignKey("Rpd_Zad");
    }
}