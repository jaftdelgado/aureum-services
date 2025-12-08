import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Asset } from '@entities/asset.entity';
import { AssetCategory } from '@entities/assetCategory.entity';
import { TeamAsset } from '@entities/teamAsset.entity';
import { Movement } from '@entities/movement.entity';

@Module({
  imports: [
    TypeOrmModule.forRoot({
      name: 'assetsConnection',
      type: 'postgres',
      url: process.env.ASSETS_DB_URL,
      entities: [Asset, AssetCategory],
      synchronize: false,
      ssl: { rejectUnauthorized: false },
    }),

    TypeOrmModule.forRoot({
      name: 'marketConnection',
      type: 'postgres',
      url: process.env.MARKET_DB_URL,
      entities: [TeamAsset, Movement],
      synchronize: false,
      ssl: { rejectUnauthorized: false },
    }),
  ],
})
export class DatabaseModule {}
