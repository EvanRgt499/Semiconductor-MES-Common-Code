
-- ============================================================
-- 一、表结构创建（核心业务表）
-- ============================================================

-- 1. 工单主表
CREATE TABLE MES_WORK_ORDER (
    WORK_ORDER_NO   VARCHAR2(30)  NOT NULL,  -- 工单编号
    PRODUCT_CODE    VARCHAR2(30)  NOT NULL,  -- 产品编码
    PRODUCT_NAME    VARCHAR2(100),           -- 产品名称
    PLAN_QTY        NUMBER(12,2),            -- 计划数量
    FINISH_QTY      NUMBER(12,2) DEFAULT 0,  -- 完成数量
    SCRAP_QTY       NUMBER(12,2) DEFAULT 0,  -- 报废数量
    STATUS          VARCHAR2(20)  DEFAULT 'Created',  -- 状态: Created/Running/Completed/Cancelled
    ROUTE_CODE      VARCHAR2(30),            -- 工艺路线编码
    LINE_CODE       VARCHAR2(30),            -- 产线编码
    PLAN_START_DATE DATE,                    -- 计划开始日期
    PLAN_END_DATE   DATE,                    -- 计划结束日期
    START_TIME      DATE,                    -- 实际开工时间
    END_TIME        DATE,                    -- 实际完工时间
    CREATE_BY       VARCHAR2(50),            -- 创建人
    CREATE_TIME     DATE DEFAULT SYSDATE,    -- 创建时间
    UPDATE_BY       VARCHAR2(50),            -- 更新人
    UPDATE_TIME     DATE,                    -- 更新时间
    CONSTRAINT PK_WORK_ORDER PRIMARY KEY (WORK_ORDER_NO)
);

-- 2. 工序报工记录表
CREATE TABLE MES_OPERATION_REPORT (
    REPORT_ID       NUMBER(15)    NOT NULL,  -- 报告ID（序列）
    WORK_ORDER_NO   VARCHAR2(30)  NOT NULL,  -- 工单编号
    OP_CODE         VARCHAR2(30)  NOT NULL,  -- 工序编码
    OP_NAME         VARCHAR2(100),           -- 工序名称
    SEQ_NO          NUMBER(4),              -- 工序顺序号
    STATION_CODE    VARCHAR2(30),            -- 工站编码
    LINE_CODE       VARCHAR2(30),            -- 产线编码
    INPUT_QTY       NUMBER(12,2),            -- 投入数量
    OUTPUT_QTY      NUMBER(12,2),            -- 产出数量
    PASS_QTY        NUMBER(12,2),            -- 良品数量
    FAIL_QTY        NUMBER(12,2),            -- 不良数量
    START_TIME      DATE,                    -- 开始时间
    END_TIME        DATE,                    -- 结束时间
    REPORT_BY       VARCHAR2(50),            -- 报工人
    REPORT_TIME     DATE DEFAULT SYSDATE,    -- 报工时间
    CONSTRAINT PK_OPERATION_REPORT PRIMARY KEY (REPORT_ID)
);

-- 3. 物料追溯表（半导体行业关键：全流程追溯）
CREATE TABLE MES_MATERIAL_TRACE (
    TRACE_ID        NUMBER(15)    NOT NULL,  -- 追溯ID
    LOT_NO          VARCHAR2(50)  NOT NULL,  -- 批次号
    MATERIAL_CODE   VARCHAR2(30)  NOT NULL,  -- 物料编码
    MATERIAL_NAME   VARCHAR2(100),           -- 物料名称
    SUPPLIER_CODE   VARCHAR2(30),            -- 供应商编码
    QTY             NUMBER(12,2),            -- 数量
    UNIT            VARCHAR2(10),            -- 单位
    WORK_ORDER_NO   VARCHAR2(30),            -- 关联工单
    OP_CODE         VARCHAR2(30),            -- 关联工序
    IN_TIME         DATE DEFAULT SYSDATE,    -- 入库时间
    SOURCE_LOT_NO   VARCHAR2(50),            -- 来源批次号
    CONSTRAINT PK_MATERIAL_TRACE PRIMARY KEY (TRACE_ID)
);

-- 4. 设备数据采集表
CREATE TABLE MES_EQUIPMENT_DATA (
    DATA_ID         NUMBER(15)    NOT NULL,  -- 数据ID
    EQ_CODE         VARCHAR2(30)  NOT NULL,  -- 设备编码
    EQ_NAME         VARCHAR2(100),           -- 设备名称
    STATUS          VARCHAR2(20),            -- 设备状态: Idle/Running/Down/PM
    CURRENT_LOT_NO  VARCHAR2(50),            -- 当前加工批次
    CURRENT_OP_CODE VARCHAR2(30),            -- 当前工序
    OUTPUT_COUNT    NUMBER(12,2),            -- 产出计数
    YIELD_COUNT     NUMBER(12,2),            -- 良品计数
    COLLECT_TIME    DATE DEFAULT SYSDATE,    -- 采集时间
    CONSTRAINT PK_EQUIPMENT_DATA PRIMARY KEY (DATA_ID)
);

-- 5. 品质检验记录表（SPC数据源）
CREATE TABLE MES_QUALITY_RECORD (
    QC_ID           NUMBER(15)    NOT NULL,  -- 检验记录ID
    LOT_NO          VARCHAR2(50)  NOT NULL,  -- 批次号
    WORK_ORDER_NO   VARCHAR2(30),            -- 工单编号
    OP_CODE         VARCHAR2(30),            -- 工序编码
    ITEM_CODE       VARCHAR2(30)  NOT NULL,  -- 检验项目编码
    ITEM_NAME       VARCHAR2(100),           -- 检验项目名称
    STD_VALUE       NUMBER(12,6),            -- 标准值
    USL             NUMBER(12,6),            -- 规格上限
    LSL             NUMBER(12,6),            -- 规格下限
    MEAS_VALUE      NUMBER(12,6),            -- 实测值
    RESULT          VARCHAR2(10),            -- 判定: Pass/Fail
    INSPECTOR       VARCHAR2(50),            -- 检验员
    INSPECT_TIME    DATE DEFAULT SYSDATE,    -- 检验时间
    CONSTRAINT PK_QUALITY_RECORD PRIMARY KEY (QC_ID)
);

-- 6. 设备OEE统计表
CREATE TABLE MES_EQUIPMENT_OEE (
    OEE_ID          NUMBER(15)    NOT NULL,
    EQ_CODE         VARCHAR2(30)  NOT NULL,
    AVAILABILITY_RATE NUMBER(6,4),           -- 可用率
    PERFORMANCE_RATE  NUMBER(6,4),           -- 表现率
    QUALITY_RATE      NUMBER(6,4),           -- 良品率
    COLLECT_TIME    DATE DEFAULT SYSDATE,
    CONSTRAINT PK_EQUIPMENT_OEE PRIMARY KEY (OEE_ID)
);

-- ============================================================
-- 二、序列（Sequence）
-- ============================================================

CREATE SEQUENCE MES_SEQ_REPORT_ID START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE MES_SEQ_TRACE_ID START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE MES_SEQ_EQ_DATA_ID START WITH 1 INCREMENT BY 1;
CREATE SEQUENCE MES_SEQ_QC_ID START WITH 1 INCREMENT BY 1;

-- ============================================================
-- 三、索引（提升查询性能）
-- ============================================================

CREATE INDEX IDX_WO_STATUS ON MES_WORK_ORDER(STATUS);
CREATE INDEX IDX_WO_LINE ON MES_WORK_ORDER(LINE_CODE);
CREATE INDEX IDX_OP_WO ON MES_OPERATION_REPORT(WORK_ORDER_NO);
CREATE INDEX IDX_OP_TIME ON MES_OPERATION_REPORT(REPORT_TIME);
CREATE INDEX IDX_TRACE_LOT ON MES_MATERIAL_TRACE(LOT_NO);
CREATE INDEX IDX_TRACE_WO ON MES_MATERIAL_TRACE(WORK_ORDER_NO);
CREATE INDEX IDX_EQ_CODE ON MES_EQUIPMENT_DATA(EQ_CODE);
CREATE INDEX IDX_EQ_TIME ON MES_EQUIPMENT_DATA(COLLECT_TIME);
CREATE INDEX IDX_QC_ITEM ON MES_QUALITY_RECORD(ITEM_CODE);
CREATE INDEX IDX_QC_TIME ON MES_QUALITY_RECORD(INSPECT_TIME);

-- ============================================================
-- 四、常用存储过程
-- ============================================================

-- 1. 工单完工处理（含良率计算和状态更新）
CREATE OR REPLACE PROCEDURE SP_COMPLETE_WORK_ORDER(
    p_wo_no     IN VARCHAR2,
    p_finish_qty IN NUMBER,
    p_scrap_qty  IN NUMBER,
    p_result    OUT VARCHAR2
) AS
    v_pass_qty  NUMBER;
    v_fail_qty  NUMBER;
    v_yield_rate NUMBER;
BEGIN
    -- 汇总各工序良品和不良数量
    SELECT NVL(SUM(PASS_QTY), 0), NVL(SUM(FAIL_QTY), 0)
    INTO v_pass_qty, v_fail_qty
    FROM MES_OPERATION_REPORT
    WHERE WORK_ORDER_NO = p_wo_no;

    -- 计算良率
    IF v_pass_qty + v_fail_qty > 0 THEN
        v_yield_rate := ROUND(v_pass_qty / (v_pass_qty + v_fail_qty) * 100, 2);
    ELSE
        v_yield_rate := 100;
    END IF;

    -- 更新工单状态
    UPDATE MES_WORK_ORDER
    SET STATUS = 'Completed',
        FINISH_QTY = p_finish_qty,
        SCRAP_QTY = p_scrap_qty,
        END_TIME = SYSDATE
    WHERE WORK_ORDER_NO = p_wo_no;

    IF SQL%ROWCOUNT > 0 THEN
        p_result := 'SUCCESS|良率:' || v_yield_rate || '%';
        COMMIT;
    ELSE
        p_result := 'FAIL|工单不存在或状态不允许完工';
        ROLLBACK;
    END IF;

EXCEPTION
    WHEN OTHERS THEN
        p_result := 'ERROR|' || SQLERRM;
        ROLLBACK;
END;
/

-- 2. 物料全流程追溯（通过批次号）
CREATE OR REPLACE PROCEDURE SP_TRACE_MATERIAL(
    p_lot_no  IN VARCHAR2,
    p_cursor  OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_cursor FOR
    SELECT
        T.TRACE_ID,
        T.LOT_NO,
        T.MATERIAL_CODE,
        T.MATERIAL_NAME,
        T.SUPPLIER_CODE,
        T.QTY,
        T.UNIT,
        T.WORK_ORDER_NO,
        T.OP_CODE,
        T.IN_TIME,
        T.SOURCE_LOT_NO
    FROM MES_MATERIAL_TRACE T
    WHERE T.LOT_NO = p_lot_no
    ORDER BY T.IN_TIME ASC;
END;
/

-- 3. 产线良率日报统计
CREATE OR REPLACE PROCEDURE SP_DAILY_YIELD_REPORT(
    p_line_code IN VARCHAR2,
    p_date      IN DATE,
    p_cursor    OUT SYS_REFCURSOR
) AS
BEGIN
    OPEN p_cursor FOR
    SELECT
        O.WORK_ORDER_NO,
        W.PRODUCT_NAME,
        O.OP_CODE,
        O.OP_NAME,
        SUM(O.INPUT_QTY)   AS TOTAL_INPUT,
        SUM(O.OUTPUT_QTY)  AS TOTAL_OUTPUT,
        SUM(O.PASS_QTY)    AS TOTAL_PASS,
        SUM(O.FAIL_QTY)    AS TOTAL_FAIL,
        ROUND(SUM(O.PASS_QTY) / NULLIF(SUM(O.OUTPUT_QTY), 0) * 100, 2) AS YIELD_RATE
    FROM MES_OPERATION_REPORT O
    LEFT JOIN MES_WORK_ORDER W ON O.WORK_ORDER_NO = W.WORK_ORDER_NO
    WHERE O.LINE_CODE = p_line_code
      AND TRUNC(O.REPORT_TIME) = TRUNC(p_date)
    GROUP BY O.WORK_ORDER_NO, W.PRODUCT_NAME, O.OP_CODE, O.OP_NAME
    ORDER BY O.OP_CODE;
END;
/

-- 4. ERP工单同步接口
CREATE OR REPLACE PROCEDURE SP_SYNC_WO_FROM_ERP(
    p_count OUT NUMBER
) AS
BEGIN
    INSERT INTO MES_WORK_ORDER
        (WORK_ORDER_NO, PRODUCT_CODE, PRODUCT_NAME, PLAN_QTY,
         STATUS, ROUTE_CODE, LINE_CODE, PLAN_START_DATE, PLAN_END_DATE,
         CREATE_BY, CREATE_TIME)
    SELECT
        E.WO_NO, E.ITEM_CODE, E.ITEM_DESC, E.QTY,
        'Created', E.ROUTE_CODE, E.LINE_CODE, E.START_DATE, E.DUE_DATE,
        'ERP_SYNC', SYSDATE
    FROM ERP_WIP_INTERFACE E
    WHERE E.PROCESS_FLAG = 'N';

    p_count := SQL%ROWCOUNT;

    -- 标记已处理
    UPDATE ERP_WIP_INTERFACE
    SET PROCESS_FLAG = 'Y',
        PROCESS_TIME = SYSDATE
    WHERE PROCESS_FLAG = 'N';

    COMMIT;
END;
/

-- ============================================================
-- 五、常用查询SQL
-- ============================================================

-- 1. 查询当前在制工单
SELECT WO.WORK_ORDER_NO, WO.PRODUCT_CODE, WO.PRODUCT_NAME,
       WO.PLAN_QTY, WO.FINISH_QTY,
       ROUND(WO.FINISH_QTY / NULLIF(WO.PLAN_QTY, 0) * 100, 2) AS COMPLETE_RATE,
       WO.LINE_CODE, WO.START_TIME
FROM MES_WORK_ORDER WO
WHERE WO.STATUS = 'Running'
ORDER BY WO.START_TIME ASC;

-- 2. 查询某工单各工序进度
SELECT OP.OP_CODE, OP.OP_NAME, OP.SEQ_NO, OP.STATION_CODE,
       OP.INPUT_QTY, OP.OUTPUT_QTY, OP.PASS_QTY, OP.FAIL_QTY,
       ROUND(OP.PASS_QTY / NULLIF(OP.OUTPUT_QTY, 0) * 100, 2) AS YIELD_RATE,
       OP.START_TIME, OP.END_TIME
FROM MES_OPERATION_REPORT OP
WHERE OP.WORK_ORDER_NO = 'WO20260406001'
ORDER BY OP.SEQ_NO ASC;

-- 3. 物料正反向追溯
-- 正向追溯: 从原料批次查使用了哪些工单/工序
SELECT T.LOT_NO, T.MATERIAL_CODE, T.MATERIAL_NAME, T.WORK_ORDER_NO, T.OP_CODE, T.IN_TIME
FROM MES_MATERIAL_TRACE T
WHERE T.LOT_NO = 'LOT20260401001'
ORDER BY T.IN_TIME;

-- 反向追溯: 从成品工单查使用了哪些原料批次
SELECT DISTINCT T.LOT_NO, T.MATERIAL_CODE, T.MATERIAL_NAME, T.SUPPLIER_CODE, T.QTY
FROM MES_MATERIAL_TRACE T
WHERE T.WORK_ORDER_NO = 'WO20260406001'
ORDER BY T.LOT_NO;

-- 4. SPC控制图数据查询（X-bar R图数据源）
SELECT
    ITEM_CODE,
    TRUNC(INSPECT_TIME, 'HH24') AS SAMPLE_HOUR,
    COUNT(*) AS SAMPLE_SIZE,
    ROUND(AVG(MEAS_VALUE), 4) AS X_BAR,
    ROUND(MAX(MEAS_VALUE) - MIN(MEAS_VALUE), 4) AS R_RANGE
FROM MES_QUALITY_RECORD
WHERE ITEM_CODE = 'THICKNESS'
  AND INSPECT_TIME BETWEEN SYSDATE - 7 AND SYSDATE
GROUP BY ITEM_CODE, TRUNC(INSPECT_TIME, 'HH24')
ORDER BY SAMPLE_HOUR;

-- 5. 设备综合效率OEE查询
SELECT
    EQ_CODE,
    TRUNC(COLLECT_TIME) AS STAT_DATE,
    ROUND(AVG(AVAILABILITY_RATE) * 100, 2) AS AVG_AVAILABILITY,
    ROUND(AVG(PERFORMANCE_RATE) * 100, 2) AS AVG_PERFORMANCE,
    ROUND(AVG(QUALITY_RATE) * 100, 2) AS AVG_QUALITY,
    ROUND(AVG(AVAILABILITY_RATE * PERFORMANCE_RATE * QUALITY_RATE) * 100, 2) AS OEE
FROM MES_EQUIPMENT_OEE
WHERE COLLECT_TIME BETWEEN SYSDATE - 30 AND SYSDATE
GROUP BY EQ_CODE, TRUNC(COLLECT_TIME)
ORDER BY EQ_CODE, STAT_DATE;

-- 6. 不良品Top N分析
SELECT
    OP.OP_CODE, OP.OP_NAME,
    D.DEFECT_CODE, D.DEFECT_NAME,
    COUNT(*) AS DEFECT_COUNT,
    ROUND(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER (PARTITION BY OP.OP_CODE), 2) AS PCT
FROM MES_DEFECT_RECORD D
JOIN MES_OPERATION_REPORT OP ON D.REPORT_ID = D.REPORT_ID
WHERE D.REPORT_TIME BETWEEN SYSDATE - 30 AND SYSDATE
GROUP BY OP.OP_CODE, OP.OP_NAME, D.DEFECT_CODE, D.DEFECT_NAME
ORDER BY DEFECT_COUNT DESC;
