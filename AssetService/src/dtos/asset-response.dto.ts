import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { AssetCategory } from '@entities/assetCategory.entity';

export class AssetResponseDto {
  @ApiProperty({ example: '9667e214-55a6-4e3e-8238-b4d7eef0b7f0' })
  publicId: string;

  @ApiProperty({ example: 'AAPL' })
  assetSymbol: string;

  @ApiProperty({ example: 'Apple Inc.' })
  assetName: string;

  @ApiProperty({ example: 'Stock', enum: ['Stock', 'Crypto', 'ETF'] })
  assetType: 'Stock' | 'Crypto' | 'ETF';

  @ApiPropertyOptional({ example: 150.5 })
  basePrice?: number;

  @ApiPropertyOptional({ example: 0.03 })
  volatility?: number;

  @ApiPropertyOptional({ example: 0.01 })
  drift?: number;

  @ApiPropertyOptional({ example: 200 })
  maxPrice?: number;

  @ApiPropertyOptional({ example: 100 })
  minPrice?: number;

  @ApiPropertyOptional({ example: 0.015 })
  dividendYield?: number;

  @ApiPropertyOptional({ example: 500000 })
  liquidity?: number;

  @ApiPropertyOptional({
    example: 'https://img.logokit.com/apple.com?token=pk_XXX',
  })
  logoUrl?: string;

  @ApiPropertyOptional({ type: () => AssetCategory })
  category?: AssetCategory;
}
