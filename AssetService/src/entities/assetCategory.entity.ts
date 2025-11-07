import { Entity, PrimaryGeneratedColumn, Column, OneToMany } from 'typeorm';
import { Asset } from './asset.entity';
import { CustomAsset } from './customAsset.entity';

@Entity('assetcategory')
export class AssetCategory {
  @PrimaryGeneratedColumn({ name: 'categoryid' })
  categoryId: number;

  @Column({ name: 'categorykey', type: 'varchar', length: 32, unique: true })
  categoryKey: string;

  @OneToMany(() => Asset, (asset) => asset.category)
  assets: Asset[];

  @OneToMany(() => CustomAsset, (customAsset) => customAsset.category)
  customAssets: CustomAsset[];
}
