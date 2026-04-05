-- 工单基本信息查询
SELECT
    wo.work_order_no,
    wo.part_no,
    wo.qty_plan,
    wo.qty_complete,
    wo.status,
    wo.line_code,
    wo.start_time,
    wo.create_time
FROM mes_work_order wo
WHERE wo.work_order_no = :work_order_no
ORDER BY wo.create_time DESC;

-- 按产线查询当日运行工单
SELECT
    work_order_no,
    part_no,
    qty_plan,
    qty_complete,
    status
FROM mes_work_order
WHERE line_code = :line_code
  AND TRUNC(create_time) = TRUNC(SYSDATE)
  AND status IN ('RELEASE','RUN');

-- 重置工单状态（现场常用修复）
UPDATE mes_work_order
SET
    status = 'RELEASE',
    update_time = SYSDATE,
    update_user = :user
WHERE work_order_no = :work_order_no;

COMMIT;