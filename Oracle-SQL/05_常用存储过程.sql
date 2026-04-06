-- 半导体封装MES系统 - 常用存储过程
-- 作者：[孟斯辰]
-- 日期：2026-04-06
-- 说明：封装常用业务逻辑，便于系统调用

-- ============================
-- 1. 重置SN工序状态存储过程
-- 用途：重置指定芯片在指定工序的过站状态并清理不良记录
-- 场景：芯片返工、设备异常后数据修复
-- 注意：此过程会同时更新过站记录和不良记录
-- ============================
CREATE OR REPLACE PROCEDURE sp_reset_sn_operation(
    p_sn            IN VARCHAR2,      -- 产品序列号
    p_op_code       IN VARCHAR2,      -- 工序代码
    p_user          IN VARCHAR2,      -- 操作人
    p_result        OUT VARCHAR2      -- 操作结果
) AS
BEGIN
    -- 1. 重置过站结果
    UPDATE mes_work_order_sn
    SET check_result = NULL,       -- 重置为未过站状态
        update_time = SYSDATE,     -- 更新时间戳
        update_user = p_user        -- 操作人
    WHERE sn = p_sn
      AND op_code = p_op_code;

    -- 2. 删除对应不良记录
    DELETE FROM mes_sn_fail
    WHERE sn = p_sn
      AND op_code = p_op_code;

    -- 3. 设置成功结果
    p_result := 'SUCCESS';
    -- 4. 提交事务
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        -- 捕获异常并返回错误信息
        p_result := 'ERROR: ' || SQLERRM;
        -- 回滚事务
        ROLLBACK;
END;
/

-- ============================
-- 调用示例
-- ============================
/*
-- 示例1：重置单个芯片的工序状态
DECLARE
    v_result VARCHAR2(100);
BEGIN
    sp_reset_sn_operation(
        p_sn => 'FAB202604060001',  -- 芯片序列号
        p_op_code => 'DB',           -- 工序代码
        p_user => 'ENG001',          -- 操作人
        p_result => v_result
    );
    DBMS_OUTPUT.PUT_LINE('操作结果: ' || v_result);
END;
*/

-- 历史记录：
-- 2026-04-06：初始版本
-- 后续计划添加：批量重置、工单状态更新等存储过程