SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';

DELETE FROM ShipmentStatusHistories;
DELETE FROM DistributionItems;
DELETE FROM DistributionRequests;
DELETE FROM TransactionItems;
DELETE FROM InventoryTransactions;
DELETE FROM TransferItems;
DELETE FROM TransferRequests;
DELETE FROM Inventories;
DELETE FROM ClassifiedBatchDonationRequests;
DELETE FROM InspectionAnswers;
DELETE FROM ClassificationResults;
DELETE FROM ClassifiedItems;
DELETE FROM ClassifiedBatches;
DELETE FROM IntakeBatchDonationRequests;
DELETE FROM PickupAssignments;
DELETE FROM IntakeBatches;
DELETE FROM TeamMembers;
DELETE FROM OperationalTeams;
DELETE FROM Shifts;
DELETE FROM WorkScheduleTemplates;
DELETE FROM Notifications;
DELETE FROM DonationRequests;
DELETE FROM CartItems;
DELETE FROM Carts;
DELETE FROM UserVerificationCodes;
DELETE FROM ProfileDetails;
DELETE FROM Profiles;
DELETE FROM Vouchers;

-- Keep every role as system configuration, but retain only Manager accounts.
DELETE u
FROM Users u
INNER JOIN Roles r ON r.Id = u.RoleId
WHERE r.RoleName <> 'Manager';

EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL';
COMMIT TRANSACTION;
