namespace BLL.DTOs;

public record DashboardMetricDto(string Key, string Label, int Count);
public record DashboardDailyDto(DateTime Date, int DonationRequests, int InboundBatches, int OutboundBatches);
public record DashboardWarehouseFlowDto(int InboundTransactions, int OutboundTransactions,
    decimal InboundWeightKg, decimal OutboundWeightKg);
public record ManagerDashboardDto(
    int TotalDonationRequests,
    int TotalIntakeBatches,
    int TotalClassifiedBatches,
    DashboardWarehouseFlowDto WarehouseFlow,
    IReadOnlyList<DashboardMetricDto> DonationRequestPipeline,
    IReadOnlyList<DashboardMetricDto> IntakeBatchPipeline,
    IReadOnlyList<DashboardMetricDto> ClassificationPipeline,
    IReadOnlyList<DashboardMetricDto> WarehouseBatchPipeline,
    string TrendGranularity,
    IReadOnlyList<DashboardDailyDto> LastSevenDays);
