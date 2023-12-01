export interface FileConvertInfo {
  id: string;
  fileName: string;
  status: FileConvertStatus;
  createDate: Date;
}

export enum FileConvertStatus
{
  Created,
  Processing,
  Completed,
  Error
}