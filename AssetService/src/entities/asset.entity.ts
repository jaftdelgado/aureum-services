import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  ManyToOne,
  JoinColumn,
} from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';
import { AssetCategory } from './assetCategory.entity';

@Entity('asset')
export class Asset {
  @ApiProperty({ description: 'ID interno del asset', example: 1 })
  @PrimaryGeneratedColumn({ name: 'assetid' })
  assetId: number;

  @ApiProperty({
    description: 'UUID público del asset',
    example: '9667e214-55a6-4e3e-8238-b4d7eef0b7f0',
  })
  @Column({
    name: 'publicid',
    type: 'uuid',
    unique: true,
    default: () => 'uuid_generate_v4()',
  })
  publicId: string;

  @ApiProperty({ description: 'Símbolo del asset', example: 'AAPL' })
  @Column({ name: 'assetsymbol', type: 'varchar', length: 12, unique: true })
  assetSymbol: string;

  @ApiProperty({ description: 'Nombre del asset', example: 'Apple Inc.' })
  @Column({ name: 'assetname', type: 'varchar', length: 32 })
  assetName: string;

  @ApiProperty({
    description: 'Tipo de asset',
    example: 'Stock',
    enum: ['Stock', 'Crypto', 'ETF'],
  })
  @Column({ name: 'assettype', type: 'varchar', length: 10 })
  assetType: 'Stock' | 'Crypto' | 'ETF';

  @ApiProperty({
    description: 'Precio base del asset',
    example: 150.5,
    required: false,
  })
  @Column({ name: 'baseprice', type: 'float', nullable: true })
  basePrice?: number;

  @ApiProperty({
    description: 'Volatilidad del asset',
    example: 0.03,
    required: false,
  })
  @Column({ name: 'volatility', type: 'float', nullable: true })
  volatility?: number;

  @ApiProperty({
    description: 'Drift del asset',
    example: 0.01,
    required: false,
  })
  @Column({ name: 'drift', type: 'float', nullable: true })
  drift?: number;

  @ApiProperty({
    description: 'Precio máximo del asset',
    example: 200,
    required: false,
  })
  @Column({ name: 'maxprice', type: 'float', nullable: true })
  maxPrice?: number;

  @ApiProperty({
    description: 'Precio mínimo del asset',
    example: 100,
    required: false,
  })
  @Column({ name: 'minprice', type: 'float', nullable: true })
  minPrice?: number;

  @ApiProperty({
    description: 'Dividend yield del asset',
    example: 0.015,
    required: false,
  })
  @Column({ name: 'dividendyield', type: 'float', nullable: true })
  dividendYield?: number;

  @ApiProperty({
    description: 'Liquidez del asset',
    example: 500000,
    required: false,
  })
  @Column({ name: 'liquidity', type: 'float', nullable: true })
  liquidity?: number;

  @ApiProperty({
    description: 'Fecha de creación del asset',
    example: '2025-12-08T07:00:00Z',
  })
  @CreateDateColumn({
    name: 'createdat',
    type: 'timestamp',
    default: () => 'CURRENT_TIMESTAMP',
  })
  createdAt: Date;

  @ApiProperty({
    description: 'URL de la imagen del asset',
    example: 'https://example.com/logo.png',
    required: false,
  })
  @Column({ name: 'assetpicurl', type: 'varchar', length: 256, nullable: true })
  assetPicUrl?: string;

  @ApiProperty({ description: 'URL generada del logo', required: false })
  logoUrl?: string;

  @ApiProperty({
    description: 'Categoría del asset',
    type: () => AssetCategory,
    required: false,
  })
  @ManyToOne(() => AssetCategory, (category) => category.assets, {
    nullable: true,
  })
  @JoinColumn({ name: 'categoryid' })
  category?: AssetCategory;
}
