﻿SET XACT_ABORT ON

BEGIN TRANSACTION
GO

/***************************************************************************************/
-- STORED PROCEDURE
--     GetInstanceBatchesByTimeStamp
--
-- DESCRIPTION
--     Divides up the instances into a configurable number of batches filter by timestamp.
--
-- PARAMETERS
--     @batchSize
--         * The desired number of instances per batch. Actual number may be smaller.
--     @batchCount
--         * The desired number of batches. Actual number may be smaller.
--     @status
--         * The instance status.
--     @startTimeStamp
--         * The start filter timestamp.
--     @endTimeStamp
--         * The end filter timestamp.
--     @maxWatermark
--         * The optional inclusive maximum watermark.
--
-- RETURN VALUE
--     The batches as defined by their inclusive minimum and maximum values.
/***************************************************************************************/
CREATE OR ALTER PROCEDURE dbo.GetInstanceBatchesByTimeStamp
    @batchSize INT,
    @batchCount INT,
    @status TINYINT,
    @startTimeStamp DATETIME,
    @endTimeStamp DATETIME,
    @maxWatermark BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON

    SELECT
        MIN(Watermark) AS MinWatermark,
        MAX(Watermark) AS MaxWatermark
    FROM
    (
        SELECT TOP (@batchSize * @batchCount)
            Watermark,
            (ROW_NUMBER() OVER(ORDER BY Watermark DESC) - 1) / @batchSize AS Batch
        FROM dbo.Instance
        WHERE Watermark <= ISNULL(@maxWatermark, Watermark)
        AND Status = @status
        AND CreatedDate >= @startTimeStamp
        AND CreatedDate <= @endTimeStamp
    ) AS I
    GROUP BY Batch
    ORDER BY Batch ASC
END
GO

COMMIT TRANSACTION

IF NOT EXISTS 
(
    SELECT *
    FROM    sys.indexes
    WHERE   NAME = 'IX_Instance_Watermark_Status_CreatedDate'
        AND Object_id = OBJECT_ID('dbo.Instance')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Instance_Watermark_Status_CreatedDate on dbo.Instance
    (
        Watermark,
        Status,
        CreatedDate
    )
    INCLUDE
    (
        PartitionKey,
        StudyInstanceUid,
        SeriesInstanceUid,
        SopInstanceUid
    )
    WITH (DATA_COMPRESSION = PAGE)
END
GO