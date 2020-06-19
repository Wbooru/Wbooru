﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Wbooru.Persistence;

namespace Wbooru.Migrations
{
    [DbContext(typeof(LocalDBContext))]
    [Migration("20200619025733_InitNewDBStuct")]
    partial class InitNewDBStuct
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5");

            modelBuilder.Entity("Wbooru.Models.Download", b =>
                {
                    b.Property<int>("DownloadId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("DisplayDownloadedLength")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DownloadFullPath")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DownloadStartTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("DownloadUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GalleryItemID")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TotalBytes")
                        .HasColumnType("INTEGER");

                    b.HasKey("DownloadId");

                    b.HasIndex("GalleryItemID");

                    b.ToTable("Downloads");
                });

            modelBuilder.Entity("Wbooru.Models.Gallery.GalleryItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DownloadFileName")
                        .HasColumnType("TEXT");

                    b.Property<string>("GalleryItemID")
                        .HasColumnType("TEXT");

                    b.Property<string>("GalleryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("PreviewImageDownloadLink")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("GalleryItems");
                });

            modelBuilder.Entity("Wbooru.Models.GalleryItemMark", b =>
                {
                    b.Property<int>("GalleryItemMarkID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ItemID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Time")
                        .HasColumnType("TEXT");

                    b.HasKey("GalleryItemMarkID");

                    b.HasIndex("ItemID");

                    b.ToTable("ItemMarks");
                });

            modelBuilder.Entity("Wbooru.Models.TagRecord", b =>
                {
                    b.Property<int>("TagID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("FromGallery")
                        .HasColumnType("TEXT");

                    b.Property<int>("RecordType")
                        .HasColumnType("INTEGER");

                    b.HasKey("TagID");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("Wbooru.Models.VisitRecord", b =>
                {
                    b.Property<int>("VisitRecordID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GalleryItemID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastVisitTime")
                        .HasColumnType("TEXT");

                    b.HasKey("VisitRecordID");

                    b.HasIndex("GalleryItemID");

                    b.ToTable("VisitRecords");
                });

            modelBuilder.Entity("Wbooru.Models.Download", b =>
                {
                    b.HasOne("Wbooru.Models.Gallery.GalleryItem", "GalleryItem")
                        .WithMany()
                        .HasForeignKey("GalleryItemID");
                });

            modelBuilder.Entity("Wbooru.Models.GalleryItemMark", b =>
                {
                    b.HasOne("Wbooru.Models.Gallery.GalleryItem", "Item")
                        .WithMany()
                        .HasForeignKey("ItemID");
                });

            modelBuilder.Entity("Wbooru.Models.TagRecord", b =>
                {
                    b.OwnsOne("Wbooru.Models.Tag", "Tag", b1 =>
                        {
                            b1.Property<int>("TagRecordTagID")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("Name")
                                .HasColumnType("TEXT");

                            b1.Property<int>("Type")
                                .HasColumnType("INTEGER");

                            b1.HasKey("TagRecordTagID");

                            b1.ToTable("Tags");

                            b1.WithOwner()
                                .HasForeignKey("TagRecordTagID");
                        });
                });

            modelBuilder.Entity("Wbooru.Models.VisitRecord", b =>
                {
                    b.HasOne("Wbooru.Models.Gallery.GalleryItem", "GalleryItem")
                        .WithMany()
                        .HasForeignKey("GalleryItemID");
                });
#pragma warning restore 612, 618
        }
    }
}