import React, { useEffect, useState } from 'react';
import { Button, Col, Popconfirm, Row, Space, Table, Tooltip, Upload } from 'antd';
import { ColumnsType, TablePaginationConfig } from 'antd/lib/table';
import Config from '../config';
import { FileConvertInfo, FileConvertStatus } from '../models/FileConvertInfo';
import { DeleteOutlined, DownloadOutlined, RetweetOutlined, UploadOutlined } from '@ant-design/icons';
import TestServiceApiService from '../services/TestServiceApiService';

interface TableParams {
    pagination?: TablePaginationConfig;
}

const MainTable = () => {
    const [data, setData] = useState();
    const [loading, setLoading] = useState(false);
    const [tableParams, setTableParams] = useState<TableParams>({
      pagination: {
        current: 1,
        pageSize: 10,
      },
    });

    const columns: ColumnsType<FileConvertInfo> = [
        {
          title: 'FileName',
          dataIndex: 'fileName',
          width: '70%',
        },
        {
          title: 'Status',
          dataIndex: 'status',
          width: '20%',
          render: (value) => {
            return FileConvertStatus[value];
          } 
        },
        {
          title: 'Actions',
          render: (_, record) => {
            return (
                <Space>
                    { record.status == FileConvertStatus.Completed &&
                        <Tooltip title="Download">
                            <DownloadOutlined 
                                onClick={async () => await TestServiceApiService.downloadConvertedResult(record.id)}/>
                        </Tooltip>}
                    { (record.status == FileConvertStatus.Completed || record.status == FileConvertStatus.Error) &&
                        <Tooltip title="Delete">
                            <Popconfirm 
                                title={`Are you sure want to delete ${record.fileName}`}
                                onConfirm={async () => {
                                    await TestServiceApiService.deleteItem(record.id);
                                    await fetchData();
                                }}>
                                <DeleteOutlined />
                            </Popconfirm>
                        </Tooltip>}
                </Space>
            );
          }
        },
    ];

    const fetchData = async () => {
        const skip = ((tableParams.pagination?.current ?? 1) - 1) * (tableParams.pagination?.pageSize ?? 0);
        const take = tableParams.pagination?.pageSize;
        const url = `${Config.ApiUrl}/Converter/GetConverterItems?skip=${skip}&take=${take}`;
        setLoading(true);
        const res = await fetch(url);
        const json = await res.json();
        setData(json.data);
        setLoading(false);
        setTableParams({
            ...tableParams,
            pagination: {
            ...tableParams.pagination,
            total: json.count
            },
        });
      };
    
    useEffect(() => {
        fetchData();
    }, [JSON.stringify(tableParams)]);

    const handleTableChange = (
        pagination: TablePaginationConfig,
      ) => {
        setTableParams({
          pagination,
        });
    };

    return (
        <>
            <Row>
                <Col xs={2}>
                    <Upload
                        action={`${Config.ApiUrl}/Converter/QueueItem`}
                        onChange ={(info) => {
                            if (info.file.status === 'done') {
                                fetchData();
                            }
                        }}>
                        <Button icon={<UploadOutlined />}>Upload</Button>
                    </Upload>
                </Col>
                <Col xs={1}>
                    <Button 
                        icon={<RetweetOutlined />}
                        onClick={fetchData}>
                        Refresh
                    </Button>
                </Col>
            </Row>
            <Table
                columns={columns}
                rowKey={record => record.id}
                dataSource={data}
                pagination={tableParams.pagination}
                onChange={handleTableChange}
                loading={loading} />
        </>
    );
}

export default MainTable;