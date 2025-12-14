using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Workshop.Infrastructure;

namespace Workshop.Infrastructure.Contexts;

public partial class WorkshopDbContext : DbContext
{
    public WorkshopDbContext()
    {
    }

    public WorkshopDbContext(DbContextOptions<WorkshopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apicredential> Apicredentials { get; set; }

    public virtual DbSet<DAccountDefinition> DAccountDefinitions { get; set; }

    public virtual DbSet<DExcelMapping> DExcelMappings { get; set; }

    public virtual DbSet<DExternalWorkshopExp> DExternalWorkshopExps { get; set; }

    public virtual DbSet<DExternalWorkshopInvoice> DExternalWorkshopInvoices { get; set; }

    public virtual DbSet<DInsuranceCompanyWorkshop> DInsuranceCompanyWorkshops { get; set; }

    public virtual DbSet<DJobCard> DJobCards { get; set; }

    public virtual DbSet<DJobCardItem> DJobCardItems { get; set; }

    public virtual DbSet<DJobCardLabor> DJobCardLabors { get; set; }

    public virtual DbSet<DJobCardLaborsProgress> DJobCardLaborsProgresses { get; set; }

    public virtual DbSet<DJobCardLineItem> DJobCardLineItems { get; set; }

    public virtual DbSet<DMaintemanceCard> DMaintemanceCards { get; set; }

    public virtual DbSet<DServiceReminder> DServiceReminders { get; set; }

    public virtual DbSet<DServiceRemindersSchedule> DServiceRemindersSchedules { get; set; }

    public virtual DbSet<DServiceRemindersStatus> DServiceRemindersStatuses { get; set; }

    public virtual DbSet<DTechnician> DTechnicians { get; set; }

    public virtual DbSet<DWorkOrderReport> DWorkOrderReports { get; set; }

    public virtual DbSet<DWorkShop> DWorkShops { get; set; }

    public virtual DbSet<DWorkshopMovement> DWorkshopMovements { get; set; }

    public virtual DbSet<DWorkshopPriceList> DWorkshopPriceLists { get; set; }

    public virtual DbSet<EntryVoucherDetail> EntryVoucherDetails { get; set; }

    public virtual DbSet<EntryVoucherHeader> EntryVoucherHeaders { get; set; }

    public virtual DbSet<GoodIssueNoteDetail> GoodIssueNoteDetails { get; set; }

    public virtual DbSet<GoodIssueNoteHeader> GoodIssueNoteHeaders { get; set; }

    public virtual DbSet<LkpEngineFuelType> LkpEngineFuelTypes { get; set; }

    public virtual DbSet<LookupDetail> LookupDetails { get; set; }

    public virtual DbSet<LookupHeader> LookupHeaders { get; set; }

    public virtual DbSet<MClaimHistory> MClaimHistories { get; set; }

    public virtual DbSet<MExcelMapping> MExcelMappings { get; set; }

    public virtual DbSet<MExternalWorkshopExp> MExternalWorkshopExps { get; set; }

    public virtual DbSet<MMovementDocument> MMovementDocuments { get; set; }

    public virtual DbSet<MTaqdeeratDocument> MTaqdeeratDocuments { get; set; }

    public virtual DbSet<MWorkOrder> MWorkOrders { get; set; }

    public virtual DbSet<MWorkOrderDetail> MWorkOrderDetails { get; set; }

    public virtual DbSet<MWorkOrdersDetailsDocument> MWorkOrdersDetailsDocuments { get; set; }

    public virtual DbSet<MWorkshopInvoice> MWorkshopInvoices { get; set; }

    public virtual DbSet<MWorkshopMovementStrike> MWorkshopMovementStrikes { get; set; }

    public virtual DbSet<TransferVouchersDetail> TransferVouchersDetails { get; set; }

    public virtual DbSet<TransferVouchersHeader> TransferVouchersHeaders { get; set; }

    public virtual DbSet<WorkOrdersService> WorkOrdersServices { get; set; }

    public virtual DbSet<WorkShopSetting> WorkShopSettings { get; set; }

    public virtual DbSet<WorkshopTransactionHistory> WorkshopTransactionHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=94.249.88.254,1433;Initial Catalog=DB_WorkshopCore;Persist Security Info=True;User ID=acs_2023;Password=ACS#DEV#2025_#;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apicredential>(entity =>
        {
            entity.ToTable("APICredential");

            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DAccountDefinition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__D_Accoun__3214EC0768883DB0");

            entity.ToTable("D_AccountDefinitions");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<DExcelMapping>(entity =>
        {
            entity.ToTable("D_Excel_Mapping");

            entity.Property(e => e.AdditionCullmanName).HasColumnName("Addition_Cullman_Name");
            entity.Property(e => e.AdditionData).HasColumnName("Addition_Data");
            entity.Property(e => e.ColumnName).HasColumnName("Column_Name");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsVatIncluded).HasColumnName("Is_Vat_Included");
            entity.Property(e => e.MappingColumnDb).HasColumnName("Mapping_ColumnDB");
            entity.Property(e => e.MappingColumnIndex).HasColumnName("Mapping_Column_Index");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Header).WithMany(p => p.DExcelMappings)
                .HasForeignKey(d => d.HeaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_D_Excel_Mapping_M_Excel_Mapping");
        });

        modelBuilder.Entity<DExternalWorkshopExp>(entity =>
        {
            entity.ToTable("D_External_Workshop_Exp");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BusinessLine)
                .HasMaxLength(550)
                .HasColumnName("Business_Line");
            entity.Property(e => e.City).HasMaxLength(550);
            entity.Property(e => e.HeaderId).HasColumnName("HeaderID");
            entity.Property(e => e.InvoiceDate)
                .HasColumnType("datetime")
                .HasColumnName("Invoice_Date");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(550)
                .HasColumnName("Invoice_No");
            entity.Property(e => e.LicensePlateNo)
                .HasMaxLength(50)
                .HasColumnName("License_Plate_No");
            entity.Property(e => e.Maker).HasMaxLength(550);
            entity.Property(e => e.Milage).HasColumnName("MILAGE");
            entity.Property(e => e.Model).HasMaxLength(550);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServiceType)
                .HasMaxLength(550)
                .HasColumnName("Service_Type");
            entity.Property(e => e.SubTotalBeforVat)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("SubTotal_BeforVat");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Vat).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VinNo)
                .HasMaxLength(550)
                .HasColumnName("Vin_No");
            entity.Property(e => e.Year).HasMaxLength(550);

            entity.HasOne(d => d.Header).WithMany(p => p.DExternalWorkshopExps)
                .HasForeignKey(d => d.HeaderId)
                .HasConstraintName("FK_D_External_Workshop_Exp_D_External_Workshop_Exp1");
        });

        modelBuilder.Entity<DExternalWorkshopInvoice>(entity =>
        {
            entity.ToTable("D_ExternalWorkshopInvoice");

            entity.HasOne(d => d.Movement).WithMany(p => p.DExternalWorkshopInvoices)
                .HasForeignKey(d => d.MovementId)
                .HasConstraintName("FK_D_workshopMovement_D_ExternalWorkshopInvoice");
        });

        modelBuilder.Entity<DInsuranceCompanyWorkshop>(entity =>
        {
            entity.ToTable("D_InsuranceCompanyWorkshop");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<DJobCard>(entity =>
        {
            entity.ToTable("D_JobCard", "JobCard");

            entity.Property(e => e.CompleteDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Discount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.IssueDate).HasColumnType("datetime");
            entity.Property(e => e.JobCardNo).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.NextOilChangeDate).HasColumnType("datetime");
            entity.Property(e => e.OdoMeter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.OilMeter).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.OriginalMeter).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ReceivedMeter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnName("startTime");
            entity.Property(e => e.Tax).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalOrder).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalParts).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalTechnicians).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.WorkshopId).HasColumnName("workshopId");
        });

        modelBuilder.Entity<DJobCardItem>(entity =>
        {
            entity.ToTable("D_JobCardItems", "JobCard");

            entity.Property(e => e.Cost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.KeyId).HasMaxLength(50);
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");

            entity.HasOne(d => d.LineItem).WithMany(p => p.DJobCardItems)
                .HasForeignKey(d => d.LineItemId)
                .HasConstraintName("FK_D_JobCardLineItemD_JobCardItems");
        });

        modelBuilder.Entity<DJobCardLabor>(entity =>
        {
            entity.ToTable("D_JobCardLabors", "JobCard");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.HourRate).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Labor).WithMany(p => p.DJobCardLabors)
                .HasForeignKey(d => d.LaborId)
                .HasConstraintName("FK_D_JobCardLabors_D_Technicians");

            entity.HasOne(d => d.LineItem).WithMany(p => p.DJobCardLabors)
                .HasForeignKey(d => d.LineItemId)
                .HasConstraintName("FK_D_JobCardLineItemD_JobCardLabors");
        });

        modelBuilder.Entity<DJobCardLaborsProgress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_D_WorkOrderLaborsProgress");

            entity.ToTable("D_JobCardLaborsProgress", "JobCard");

            entity.Property(e => e.LaborLineItemId).HasColumnName("LaborLineItemID");
            entity.Property(e => e.PauseOrFinish).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.WorkingHour).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.LaborLineItem).WithMany(p => p.DJobCardLaborsProgresses)
                .HasForeignKey(d => d.LaborLineItemId)
                .HasConstraintName("FK_D_LaborProgressD_JobCardLabors");
        });

        modelBuilder.Entity<DJobCardLineItem>(entity =>
        {
            entity.ToTable("D_JobCardLineItem", "JobCard");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.TotalLaborCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalLaborTax).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalPartsCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalPartsTax).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.JobCard).WithMany(p => p.DJobCardLineItems)
                .HasForeignKey(d => d.JobCardId)
                .HasConstraintName("FK_D_JobCardLineItem_D_JobCard");
        });

        modelBuilder.Entity<DMaintemanceCard>(entity =>
        {
            entity.ToTable("D_MaintemanceCard");

            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.DMaintemanceCards)
                .HasForeignKey(d => d.WorkOrderId)
                .HasConstraintName("FK_D_WorkOrders_D_MaintemanceCard");
        });

        modelBuilder.Entity<DServiceReminder>(entity =>
        {
            entity.ToTable("D_ServiceReminders");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.StartMeter).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<DServiceRemindersSchedule>(entity =>
        {
            entity.ToTable("D_ServiceRemindersSchedule");

            entity.Property(e => e.CompletedMeter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.DuePrimaryMeter).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.PrimaryMeter).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.ServiceReminder).WithMany(p => p.DServiceRemindersSchedules)
                .HasForeignKey(d => d.ServiceReminderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_D_ServiceRemindersSchedule_D_ServiceReminders");
        });

        modelBuilder.Entity<DServiceRemindersStatus>(entity =>
        {
            entity.ToTable("D_ServiceRemindersStatus");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<DTechnician>(entity =>
        {
            entity.ToTable("D_Technicians");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.PrimaryAddress).HasMaxLength(50);
            entity.Property(e => e.PrimaryName).HasMaxLength(50);
            entity.Property(e => e.SecondaryAddress).HasMaxLength(50);
            entity.Property(e => e.SecondaryName).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<DWorkOrderReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_WorkOrder.D_WorkOrderReport");

            entity.ToTable("D_WorkOrderReport", "WorkOrder");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.DWorkOrderReports)
                .HasForeignKey(d => d.WorkOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_D_WorkOrderReport_D_WorkOrder");
        });

        modelBuilder.Entity<DWorkShop>(entity =>
        {
            entity.ToTable("D_WorkShop");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.GoogleUrl).HasColumnName("GoogleURL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<DWorkshopMovement>(entity =>
        {
            entity.HasKey(e => e.MovementId);

            entity.ToTable("D_WorkshopMovement");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExitMeter).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GregorianMovementDate).HasColumnType("datetime");
            entity.Property(e => e.GregorianMovementEndDate).HasColumnType("datetime");
            entity.Property(e => e.HijriMovementDate).HasColumnName("hijriMovementDate");
            entity.Property(e => e.IsRegularMaintenance).HasColumnName("isRegularMaintenance");
            entity.Property(e => e.IshijriMovement).HasColumnName("ishijriMovement");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.MovementIn).HasColumnName("MovementIN");
            entity.Property(e => e.ReceivedMeter).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
        });

        modelBuilder.Entity<DWorkshopPriceList>(entity =>
        {
            entity.HasKey(e => new { e.WorkshopId, e.ItemId });

            entity.ToTable("D_WorkshopPriceList");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");
        });

        modelBuilder.Entity<EntryVoucherDetail>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HeaderId).HasColumnName("Header_Id");
            entity.Property(e => e.KeyId).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.UnitQuantity).HasColumnType("decimal(18, 3)");
        });

        modelBuilder.Entity<EntryVoucherHeader>(entity =>
        {
            entity.ToTable("EntryVoucherHeader");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<GoodIssueNoteDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoodIssu__3214EC075AFF6092");

            entity.Property(e => e.HeaderId).HasColumnName("Header_Id");
            entity.Property(e => e.KeyId).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.UnitQuantity).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.Header).WithMany(p => p.GoodIssueNoteDetails)
                .HasForeignKey(d => d.HeaderId)
                .HasConstraintName("FK__GoodIssue__Heade__756D6ECB");
        });

        modelBuilder.Entity<GoodIssueNoteHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoodIsss__3214EC0738C85FC7");

            entity.ToTable("GoodIssueNoteHeader");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<LkpEngineFuelType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LKP_Engi__3214EC07ABDF0CDD");

            entity.ToTable("LKP_EngineFuelType");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TypeNameAr)
                .HasMaxLength(50)
                .HasColumnName("TypeName_Ar");
            entity.Property(e => e.TypeNameEn)
                .HasMaxLength(50)
                .HasColumnName("TypeName_En");
        });

        modelBuilder.Entity<LookupDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lookup_D__3214EC07B5FB0A9D");

            entity.ToTable("Lookup_Details");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.Primaryname).HasMaxLength(255);
            entity.Property(e => e.SeconderyName).HasMaxLength(255);

            entity.HasOne(d => d.Header).WithMany(p => p.LookupDetails)
                .HasForeignKey(d => d.HeaderId)
                .HasConstraintName("FK__Lookup_De__Heade__15FA39EE");
        });

        modelBuilder.Entity<LookupHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lookup_H__3214EC079A3E02E9");

            entity.ToTable("Lookup_Headers");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(255);
            entity.Property(e => e.PrimaryName).HasMaxLength(255);
            entity.Property(e => e.SecondaryName).HasMaxLength(255);
        });

        modelBuilder.Entity<MClaimHistory>(entity =>
        {
            entity.ToTable("M_ClaimHistory");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MExcelMapping>(entity =>
        {
            entity.ToTable("M_Excel_Mapping");

            entity.HasIndex(e => e.WorkshopId, "IX_M_Excel_Mapping").IsUnique();

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.StartedColumn).HasColumnName("Started_Column");
            entity.Property(e => e.StartedRow).HasColumnName("Started_Row");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.WorkshopId).HasColumnName("workshopId");
        });

        modelBuilder.Entity<MExternalWorkshopExp>(entity =>
        {
            entity.ToTable("M_External_Workshop_Exp");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExcelDate)
                .HasColumnType("datetime")
                .HasColumnName("Excel_Date");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<MMovementDocument>(entity =>
        {
            entity.ToTable("M_MovementDocuments");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Movement).WithMany(p => p.MMovementDocuments)
                .HasForeignKey(d => d.MovementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_M_MovementM_MovementDocuments");
        });

        modelBuilder.Entity<MTaqdeeratDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__M_Taqdee__3214EC07F2894B88");

            entity.ToTable("M_TaqdeeratDocuments", "WorkOrder");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.TaqFileName).HasMaxLength(50);

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.MTaqdeeratDocuments)
                .HasForeignKey(d => d.WorkOrderId)
                .HasConstraintName("FK__M_Taqdeer__WorkOrder__2630A1B7");
        });

        modelBuilder.Entity<MWorkOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_M_Dameges");

            entity.ToTable("M_WorkOrders", "WorkOrder");

            entity.Property(e => e.ActualTotalCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ClaimAmountReceivedDate).HasColumnType("datetime");
            entity.Property(e => e.CliamNumber).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.EstimateAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.EstimatedAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.EstimationFees).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ExternalWsprice)
                .HasColumnType("decimal(18, 3)")
                .HasColumnName("ExternalWSPrice");
            entity.Property(e => e.FkAgreementId).HasColumnName("FK_AgreementId");
            entity.Property(e => e.FkVehicleMovementId).HasColumnName("FK_VehicleMovementId");
            entity.Property(e => e.GregorianDamageDate).HasColumnType("datetime");
            entity.Property(e => e.HijriDamagetDate).HasColumnName("hijriDamagetDate");
            entity.Property(e => e.InsurancePricing).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
            entity.Property(e => e.ReceivedKm)
                .HasColumnType("decimal(18, 3)")
                .HasColumnName("ReceivedKM");
            entity.Property(e => e.TaqdeeratPrice).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.ThereIsAsecondParty).HasColumnName("ThereIsASecondParty");
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalInsurance).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TowingFees).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Wfstatus).HasColumnName("WFStatus");
            entity.Property(e => e.WorkOrderNo).HasDefaultValue(0);
            entity.Property(e => e.WorkOrderStatus).HasDefaultValue(1);
            entity.Property(e => e.Wsprice)
                .HasColumnType("decimal(18, 3)")
                .HasColumnName("WSPrice");
        });

        modelBuilder.Entity<MWorkOrderDetail>(entity =>
        {
            entity.ToTable("M_WorkOrderDetails", "WorkOrder");

            entity.Property(e => e.IsFix).HasColumnName("isFix");

            entity.HasOne(d => d.WorkOrder).WithMany(p => p.MWorkOrderDetails)
                .HasForeignKey(d => d.WorkOrderId)
                .HasConstraintName("FK_M_WorkOrderDetails_M_WorkOrders");
        });

        modelBuilder.Entity<MWorkOrdersDetailsDocument>(entity =>
        {
            entity.ToTable("M_WorkOrdersDetailsDocument", "WorkOrder");

            entity.HasOne(d => d.WorkOrderDetails).WithMany(p => p.MWorkOrdersDetailsDocuments)
                .HasForeignKey(d => d.WorkOrderDetailsId)
                .HasConstraintName("FK_M_WorkOrdersDetailsDocument_M_WorkOrderDetails");
        });

        modelBuilder.Entity<MWorkshopInvoice>(entity =>
        {
            entity.ToTable("M_WorkshopInvoice");

            entity.Property(e => e.ConsumptionValueOfSpareParts).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.DeductibleAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.InvoiceDate)
                .HasColumnType("datetime")
                .HasColumnName("Invoice_Date");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.LaborCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.PartsCost).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Vat).HasColumnType("decimal(18, 3)");

            entity.HasOne(d => d.Movement).WithMany(p => p.MWorkshopInvoices)
                .HasForeignKey(d => d.MovementId)
                .HasConstraintName("FK_M_WorkshopMovementM_WorkshopInvoice");
        });

        modelBuilder.Entity<MWorkshopMovementStrike>(entity =>
        {
            entity.ToTable("M_WorkshopMovementStrikes");

            entity.HasOne(d => d.Movement).WithMany(p => p.MWorkshopMovementStrikes)
                .HasForeignKey(d => d.MovementId)
                .HasConstraintName("FK_M_WorkshopMovementM_WorkshopMovementStrikes");
        });

        modelBuilder.Entity<TransferVouchersDetail>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.HeaderId).HasColumnName("Header_Id");
            entity.Property(e => e.KeyId).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.UnitQuantity).HasColumnType("decimal(18, 3)");
        });

        modelBuilder.Entity<TransferVouchersHeader>(entity =>
        {
            entity.ToTable("TransferVouchersHeader");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.ModifyAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<WorkOrdersService>(entity =>
        {
            entity.ToTable("WorkOrdersServices", "WorkOrder");

            entity.Property(e => e.AccServiceId).HasColumnName("acc_ServiceId");
            entity.Property(e => e.CardDescription)
                .HasMaxLength(500)
                .HasColumnName("cardDescription");
            entity.Property(e => e.VcVehicleId).HasColumnName("vc_VehicleId");
            entity.Property(e => e.WsWorkOrderId).HasColumnName("ws_WorkOrderId");
        });

        modelBuilder.Entity<WorkshopTransactionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_WorkshoptransactionHistory");

            entity.ToTable("WorkshopTransactionHistory");

            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.WorkshopId).HasColumnName("workshopId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
