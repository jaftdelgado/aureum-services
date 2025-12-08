import { Entity, PrimaryGeneratedColumn, Column, OneToMany } from 'typeorm';
import { ApiProperty } from '@nestjs/swagger';
import { Asset } from './asset.entity';

@Entity('assetcategory')
export class AssetCategory {
  @ApiProperty({ description: 'ID interno de la categoría', example: 1 })
  @PrimaryGeneratedColumn({ name: 'categoryid' })
  categoryId: number;

  @ApiProperty({
    description: 'Clave única de la categoría',
    example: 'tech_stocks',
  })
  @Column({ name: 'categorykey', type: 'varchar', length: 32, unique: true })
  categoryKey: string;

  @ApiProperty({
    description: 'Lista de assets asociados a esta categoría',
    type: () => [Asset],
    required: false,
  })
  @OneToMany(() => Asset, (asset) => asset.category)
  assets: Asset[];
}
