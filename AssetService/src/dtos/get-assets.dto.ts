import { IsOptional, IsString, IsNumber, IsEnum, Min } from 'class-validator';
import { Type } from 'class-transformer';

export class GetAssetsDto {
  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  page?: number = 1;

  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  limit?: number = 10;

  @IsOptional()
  @IsString()
  search?: string;

  @IsOptional()
  @IsEnum(['Stock', 'Crypto', 'ETF'])
  assetType?: 'Stock' | 'Crypto' | 'ETF';

  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(0)
  basePrice?: number;

  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  categoryId?: number;

  @IsOptional()
  @IsEnum(['ASC', 'DESC'])
  orderByBasePrice?: 'ASC' | 'DESC';

  @IsOptional()
  @IsEnum(['ASC', 'DESC'])
  orderByAssetName?: 'ASC' | 'DESC';
}
