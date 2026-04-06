-- ===============================================
-- 常用存储过程
-- 功能：重置SN工序状态并删除对应不良记录
-- 优化：添加注释、参数化查询、性能优化
-- ===============================================

-- ------------------------------------------------
-- 重置SN工序状态存储过程
-- 功能：重置指定SN在指定工序的过站状态并删除对应不良记录
-- 参数：
--   p_sn - 产品序列号（输入）
--   p_op_code - 工序代码（输入）
--   p_user - 操作人（输入）
--   p_result - 操作结果（输出）
-- 性能优化：建议在(sn, op_code)上创建复合索引
-- ------------------------------------------------
CREATE OR REPLACE PROCEDURE sp_reset_sn_operation(
    p_sn            IN VARCHAR2,      -- 产品序列号
    p_op_code       IN VARCHAR2,      -- 工序代码
    p_user          IN VARCHAR2,      -- 操作人
    p_result        OUT VARCHAR2      -- 操作结果
) AS
BEGIN
    -- 重置过站结果
    UPDATE mes_work_order_sn
    SET check_result = NULL,       -- 重置为未过站状态
        update_time = SYSDATE,     -- 更新时间为当前系统时间
        update_user = p_user        -- 操作人
    WHERE sn = p_sn
      AND op_code = p_op_code;

    -- 删除对应不良记录
    DELETE FROM mes_sn_fail
    WHERE sn = p_sn
      AND op_code = p_op_code;

    -- 设置成功结果
    p_result := 'SUCCESS';
    -- 提交事务
    COMMIT;

EXCEPTION
    WHEN OTHERS THEN
        -- 捕获异常并返回错误信息
        p_result := 'ERROR: ' || SQLERRM;
        -- 回滚事务
        ROLLBACK;
END;
/

-- ------------------------------------------------
-- 调用示例
-- ------------------------------------------------
/*
DECLARE
    v_result VARCHAR2(100);
BEGIN
    sp_reset_sn_operation(
        p_sn => 'SN123456',
        p_op_code => 'OP01',
        p_user => 'ADMIN',
        p_result => v_result
    );
    DBMS_OUTPUT.PUT_LINE('操作结果: ' || v_result);
END;
*/