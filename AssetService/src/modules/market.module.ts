import { Module, forwardRef } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { TeamAsset } from '@entities/teamAsset.entity';
import { TeamAssetController } from '@controllers/teamAsset.controller';
import { TeamAssetService } from '@services/teamAsset.service';
import { AssetsModule } from '@modules/assets.module';

@Module({
  imports: [
    TypeOrmModule.forFeature([TeamAsset], 'marketConnection'),
    forwardRef((): typeof AssetsModule => AssetsModule),
  ],
  controllers: [TeamAssetController],
  providers: [TeamAssetService],
  exports: [TeamAssetService],
})
export class MarketModule {}
