import {
  IsOptional,
  IsString,
  IsNumber,
  IsArray,
  IsEnum,
  Min,
  IsUUID,
} from 'class-validator';
import { Type } from 'class-transformer';
import { ApiPropertyOptional } from '@nestjs/swagger';

export class GetAssetsDto {
  @ApiPropertyOptional({ description: 'Número de página', default: 1 })
  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  page?: number = 1;

  @ApiPropertyOptional({
    description: 'Cantidad de items por página',
    default: 10,
  })
  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  limit?: number = 10;

  @ApiPropertyOptional({
    description: 'Término de búsqueda para nombre o símbolo del asset',
  })
  @IsOptional()
  @IsString()
  search?: string;

  @ApiPropertyOptional({
    description: 'Tipo de asset',
    enum: ['Stock', 'Crypto', 'ETF'],
  })
  @IsOptional()
  @IsEnum(['Stock', 'Crypto', 'ETF'])
  assetType?: 'Stock' | 'Crypto' | 'ETF';

  @ApiPropertyOptional({
    description: 'Filtrar por precio base mínimo',
    minimum: 0,
  })
  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(0)
  basePrice?: number;

  @ApiPropertyOptional({
    description: 'Filtrar por ID de categoría',
    minimum: 1,
  })
  @IsOptional()
  @Type(() => Number)
  @IsNumber()
  @Min(1)
  categoryId?: number;

  @ApiPropertyOptional({
    description: 'Ordenar por precio base',
    enum: ['ASC', 'DESC'],
  })
  @IsOptional()
  @IsEnum(['ASC', 'DESC'])
  orderByBasePrice?: 'ASC' | 'DESC';

  @ApiPropertyOptional({
    description: 'Ordenar por nombre del asset',
    enum: ['ASC', 'DESC'],
  })
  @IsOptional()
  @IsEnum(['ASC', 'DESC'])
  orderByAssetName?: 'ASC' | 'DESC';

  @ApiPropertyOptional({
    description: 'IDs de assets seleccionados',
    type: [String],
  })
  @IsOptional()
  @IsArray()
  @IsUUID('4', { each: true })
  selectedAssetIds?: string[];
}
