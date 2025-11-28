import { IsString, IsNumber, IsOptional, IsIn } from 'class-validator';

export class CreateAssetDto {
  @IsString()
  assetSymbol: string;

  @IsString()
  assetName: string;

  @IsIn(['Stock', 'Crypto', 'ETF'], {
    message: 'assetType must be one of: Stock, Crypto, ETF',
  })
  assetType: 'Stock' | 'Crypto' | 'ETF';

  @IsOptional()
  @IsNumber()
  basePrice?: number;

  @IsOptional()
  @IsNumber()
  volatility?: number;

  @IsOptional()
  @IsNumber()
  drift?: number;

  @IsOptional()
  @IsNumber()
  maxPrice?: number;

  @IsOptional()
  @IsNumber()
  minPrice?: number;

  @IsOptional()
  @IsNumber()
  dividendYield?: number;

  @IsOptional()
  @IsNumber()
  liquidity?: number;

  @IsOptional()
  @IsString()
  assetPicUrl?: string;

  @IsOptional()
  @IsNumber()
  categoryId?: number;
}
