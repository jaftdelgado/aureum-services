import {
  Entity,
  PrimaryGeneratedColumn,
  Column,
  CreateDateColumn,
  ManyToOne,
  JoinColumn,
} from 'typeorm';
import { AssetCategory } from './assetCategory.entity';

@Entity('asset')
export class Asset {
  @PrimaryGeneratedColumn({ name: 'assetid' })
  assetId: number;

  @Column({
    name: 'publicid',
    type: 'uuid',
    unique: true,
    default: () => 'uuid_generate_v4()',
  })
  publicId: string;

  @Column({ name: 'assetsymbol', type: 'varchar', length: 12, unique: true })
  assetSymbol: string;

  @Column({ name: 'assetname', type: 'varchar', length: 32 })
  assetName: string;

  @Column({ name: 'assettype', type: 'varchar', length: 10 })
  assetType: 'Stock' | 'Crypto' | 'ETF';

  @Column({ name: 'baseprice', type: 'float', nullable: true })
  basePrice?: number;

  @Column({ name: 'volatility', type: 'float', nullable: true })
  volatility?: number;

  @Column({ name: 'drift', type: 'float', nullable: true })
  drift?: number;

  @Column({ name: 'maxprice', type: 'float', nullable: true })
  maxPrice?: number;

  @Column({ name: 'minprice', type: 'float', nullable: true })
  minPrice?: number;

  @Column({ name: 'dividendyield', type: 'float', nullable: true })
  dividendYield?: number;

  @Column({ name: 'liquidity', type: 'float', nullable: true })
  liquidity?: number;

  @CreateDateColumn({
    name: 'createdat',
    type: 'timestamp',
    default: () => 'CURRENT_TIMESTAMP',
  })
  createdAt: Date;

  @Column({ name: 'assetpicurl', type: 'varchar', length: 256, nullable: true })
  assetPicUrl?: string;

  @ManyToOne(() => AssetCategory, (category) => category.assets, {
    nullable: true,
  })
  @JoinColumn({ name: 'categoryid' })
  category?: AssetCategory;
}
